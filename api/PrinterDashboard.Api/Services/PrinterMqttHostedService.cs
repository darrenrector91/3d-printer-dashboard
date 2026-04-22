using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PrinterDashboard.Api.Configuration;
using PrinterDashboard.Api.Services.Interfaces;

namespace PrinterDashboard.Api.Services;

public sealed class PrinterMqttHostedService : BackgroundService, IPrinterMqttClientService
{
    private readonly BambuPrinterOptions _options;
    private readonly ILogger<PrinterMqttHostedService> _logger;

    public bool IsConnected { get; private set; }

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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Printer MQTT hosted service stopping.");
        IsConnected = false;
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Printer MQTT hosted service is running for host {Host}.", _options.Host);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}