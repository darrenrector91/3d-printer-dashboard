using PrinterDashboard.Api.Models;

namespace PrinterDashboard.Api.Services.Interfaces;

public interface IPrinterMqttClientService
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    PrinterStatus CurrentStatus { get; }
    bool IsConnected { get; }
    DateTime? LastMessageAtUtc { get; }
    string? LastMessageTopic { get; }
    string? LastError { get; }

}