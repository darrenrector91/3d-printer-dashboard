using System.Security.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using PrinterDashboard.Api.Configuration;
using PrinterDashboard.Api.Services.Interfaces;

namespace PrinterDashboard.Api.Services;

public sealed class PrinterMqttHostedService : BackgroundService, IPrinterMqttClientService
{
    private readonly BambuPrinterOptions _options;
    private readonly ILogger<PrinterMqttHostedService> _logger;
    private IMqttClient? _client;
    private MqttClientOptions? _mqttOptions;
    private CancellationToken _stoppingToken;

    public bool IsConnected => _client?.IsConnected == true;

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

    Task IPrinterMqttClientService.StartAsync(CancellationToken cancellationToken)
    {
        return StartAsync(cancellationToken);
    }

    Task IPrinterMqttClientService.StopAsync(CancellationToken cancellationToken)
    {
        return StopAsync(cancellationToken);
    }
}