using PrinterDashboard.Api.Services;

namespace PrinterDashboard.Api.Services.Interfaces;

public interface IBambuMqttService
{
    Task<MqttConnectionTestResult> TryConnectAsync(CancellationToken cancellationToken = default);
}