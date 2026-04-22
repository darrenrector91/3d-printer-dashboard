namespace PrinterDashboard.Api.Services;

public sealed class MqttConnectionTestResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}