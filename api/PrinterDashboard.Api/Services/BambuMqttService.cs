using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MQTTnet;
using PrinterDashboard.Api.Configuration;
using PrinterDashboard.Api.Services.Interfaces;

namespace PrinterDashboard.Api.Services;

public sealed class BambuMqttService : IBambuMqttService
{
    private readonly BambuPrinterOptions _options;
    private readonly ILogger<BambuMqttService> _logger;

    public BambuMqttService(
        IOptions<BambuPrinterOptions> options,
        ILogger<BambuMqttService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MqttConnectionTestResult> TryConnectAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            return new MqttConnectionTestResult
            {
                Success = false,
                Message = "Printer host is not configured."
            };
        }

        if (string.IsNullOrWhiteSpace(_options.AccessCode))
        {
            return new MqttConnectionTestResult
            {
                Success = false,
                Message = "Printer access code is not configured."
            };
        }

        if (string.IsNullOrWhiteSpace(_options.Username))
        {
            return new MqttConnectionTestResult
            {
                Success = false,
                Message = "Printer username is not configured."
            };
        }

        try
        {
            var factory = new MqttClientFactory();
            using var client = factory.CreateMqttClient();

            var mqttOptions = new MqttClientOptionsBuilder()
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

            var response = await client.ConnectAsync(mqttOptions, cancellationToken);

            if (response.ResultCode == MqttClientConnectResultCode.Success)
            {
                await client.DisconnectAsync(cancellationToken: cancellationToken);

                return new MqttConnectionTestResult
                {
                    Success = true,
                    Message = "Successfully connected to the printer MQTT broker over LAN."
                };
            }

            return new MqttConnectionTestResult
            {
                Success = false,
                Message = $"MQTT connection failed: {response.ResultCode}."
            };
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning(ex, "TLS authentication failed while connecting to the printer.");

            return new MqttConnectionTestResult
            {
                Success = false,
                Message = "TLS handshake failed while connecting to the printer."
            };
        }
        catch (OperationCanceledException)
        {
            return new MqttConnectionTestResult
            {
                Success = false,
                Message = "MQTT connection attempt timed out."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected MQTT connection failure.");

            return new MqttConnectionTestResult
            {
                Success = false,
                Message = $"MQTT connection failed: {Sanitize(ex.Message)}"
            };
        }
    }

    private static string Sanitize(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Unknown error.";
        }

        return message
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();
    }
}