namespace PrinterDashboard.Api.Services.Interfaces;

public interface IPrinterMqttClientService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    bool IsConnected { get; }
}