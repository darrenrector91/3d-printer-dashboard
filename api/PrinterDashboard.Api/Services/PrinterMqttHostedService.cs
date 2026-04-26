using System.Text;
using System.Security.Authentication;
using System.Buffers;
using Microsoft.Extensions.Options;
using MQTTnet;
using PrinterDashboard.Api.Configuration;
using PrinterDashboard.Api.Services.Interfaces;
using System.Text.Json;
using PrinterDashboard.Api.Models;

namespace PrinterDashboard.Api.Services;

public sealed class PrinterMqttHostedService : BackgroundService, IPrinterMqttClientService
{
    private readonly BambuPrinterOptions _options;
    private readonly ILogger<PrinterMqttHostedService> _logger;
    private readonly object _lock = new();
    private IMqttClient? _client;
    private MqttClientOptions? _mqttOptions;
    private CancellationToken _stoppingToken;
    private DateTime _lastLogTime = DateTime.MinValue;

    public bool IsConnected => _client?.IsConnected == true;
    public DateTime? LastMessageAtUtc { get; private set; }
    public string? LastMessageTopic { get; private set; }
    public string? LastError { get; private set; }
    public PrinterStatus CurrentStatus { get; private set; } = new();

    public PrinterMqttHostedService(
        IOptions<BambuPrinterOptions> options,
        ILogger<PrinterMqttHostedService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Printer MQTT hosted service starting.");
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Printer MQTT hosted service stopping.");

        if (_client?.IsConnected == true)
        {
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogWarning("Printer MQTT host is not configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogWarning("Printer MQTT username is not configured.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.AccessCode))
        {
            _logger.LogWarning("Printer MQTT access code is not configured.");
            return;
        }

        try
        {
            var factory = new MqttClientFactory();
            _client = factory.CreateMqttClient();

            _client.ApplicationMessageReceivedAsync += async args =>
             {
                 var topic = args.ApplicationMessage.Topic;

                 // track status FIRST
                 LastMessageAtUtc = DateTime.UtcNow;
                 LastMessageTopic = topic;

                 if (string.IsNullOrWhiteSpace(topic) || !topic.Contains("/report"))
                 {
                     return;
                 }

                 var now = DateTime.UtcNow;

                 if ((now - _lastLogTime).TotalSeconds < 60)
                 {
                     return;
                 }

                 _lastLogTime = now;

                 var payloadSequence = args.ApplicationMessage.Payload;

                 var payload = payloadSequence.Length > 0
                    ? Encoding.UTF8.GetString(payloadSequence.ToArray())
                    : string.Empty;

                 try
                 {
                     using var doc = JsonDocument.Parse(payload);

                     if (doc.RootElement.TryGetProperty("print", out var print))
                     {
                         var rawState = print.TryGetProperty("gcode_state", out var s)
                             ? s.GetString()
                             : null;

                         var state = rawState switch
                         {
                             "RUNNING" => PrinterState.Printing,
                             "PAUSE" => PrinterState.Paused,
                             "IDLE" => PrinterState.Idle,
                             _ => PrinterState.Unknown
                         };
                         var progress = print.TryGetProperty("mc_percent", out var p) ? p.GetInt32() : 0;
                         var nozzleTemp = print.TryGetProperty("nozzle_temper", out var n) ? n.GetDouble() : 0;
                         var bedTemp = print.TryGetProperty("bed_temper", out var b) ? b.GetDouble() : 0;

                         lock (_lock)
                         {
                             CurrentStatus.State = state;
                             CurrentStatus.ProgressPercent = progress;
                             CurrentStatus.NozzleTemperature = nozzleTemp;
                             CurrentStatus.BedTemperature = bedTemp;
                             CurrentStatus.LastUpdatedUtc = DateTime.UtcNow;
                         }

                         _logger.LogInformation(
                             "Printer: {State} | {Progress}% | Nozzle {Nozzle}°C | Bed {Bed}°C",
                             state,
                             progress,
                             nozzleTemp,
                             bedTemp);
                     }
                 }
                 catch
                 {
                 }

                 _logger.LogInformation(
                    "MQTT report received. Topic: {Topic}. Size: {Size} bytes.",
                    topic,
                    payload.Length);

                 if (!string.IsNullOrWhiteSpace(payload) && payload.Contains("\"print\""))
                 {
                     var sampleDirectory = Path.Combine(AppContext.BaseDirectory, "mqtt-samples");
                     Directory.CreateDirectory(sampleDirectory);

                     var fileName = $"mqtt-{now:yyyyMMdd-HHmmss-fff}.json";
                     var filePath = Path.Combine(sampleDirectory, fileName);

                     await File.WriteAllTextAsync(filePath, payload, _stoppingToken);

                     _logger.LogInformation("Saved MQTT sample to {FilePath}", filePath);
                 }
             };
            _client.DisconnectedAsync += async args =>
            {
                if (_stoppingToken.IsCancellationRequested)
                {
                    return;
                }

                _logger.LogWarning("Printer MQTT disconnected. Reconnect will be attempted.");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), _stoppingToken);

                    if (_client?.IsConnected == false && _mqttOptions is not null)
                    {
                        var reconnectResponse = await _client.ConnectAsync(_mqttOptions, _stoppingToken);

                        if (reconnectResponse.ResultCode == MqttClientConnectResultCode.Success)
                        {
                            _logger.LogInformation("Reconnected to printer MQTT broker at {Host}:{Port}.", _options.Host, _options.Port);
                            await SubscribeToTopicsAsync(_stoppingToken);
                        }
                        else
                        {
                            _logger.LogWarning("Printer MQTT reconnect failed with result code {ResultCode}.", reconnectResponse.ResultCode);
                        }
                    }
                }
                catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Reconnect cancelled because hosted service is stopping.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while reconnecting to printer MQTT.");
                }
            };

            _mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithCredentials(_options.Username, _options.AccessCode)
                .WithTimeout(TimeSpan.FromSeconds(10))
                .WithTlsOptions(tls =>
                {
                    tls.UseTls();
                    tls.WithSslProtocols(SslProtocols.Tls12);
                    tls.WithCertificateValidationHandler(_ => true);
                })
                .Build();

            var response = await _client.ConnectAsync(_mqttOptions, stoppingToken);

            if (response.ResultCode == MqttClientConnectResultCode.Success)
            {
                _logger.LogInformation("Connected to printer MQTT broker at {Host}:{Port}.", _options.Host, _options.Port);
                await SubscribeToTopicsAsync(stoppingToken);
            }
            else
            {
                _logger.LogWarning("Printer MQTT connection failed with result code {ResultCode}.", response.ResultCode);
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Printer MQTT hosted service cancellation requested.");
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning(ex, "TLS authentication failed while connecting to printer MQTT.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while starting printer MQTT connection.");
        }
    }

    private async Task SubscribeToTopicsAsync(CancellationToken cancellationToken)
    {
        if (_client is null)
        {
            return;
        }

        var topics = new[]
        {
            "device/+/report",
            "device/+/request",
            "device/+/heartbeat"
        };

        foreach (var topic in topics)
        {
            await _client.SubscribeAsync(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce, cancellationToken);
            _logger.LogInformation("Subscribed to MQTT topic {Topic}.", topic);
        }
    }

    Task IPrinterMqttClientService.StartAsync(CancellationToken cancellationToken)
    {
        return StartAsync(cancellationToken);
    }

    Task IPrinterMqttClientService.StopAsync(CancellationToken cancellationToken)
    {
        return StopAsync(cancellationToken);
    }
}