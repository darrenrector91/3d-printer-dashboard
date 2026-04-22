namespace PrinterDashboard.Api.Services;

public interface IBambuMqttService
{
    Task<MqttConnectionTestResult> TryConnectAsync(CancellationToken cancellationToken = default);
}