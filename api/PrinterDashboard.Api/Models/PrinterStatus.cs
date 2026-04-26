namespace PrinterDashboard.Api.Models;

public sealed class PrinterStatus
{
    public string? State { get; set; }
    public int ProgressPercent { get; set; }
    public double NozzleTemperature { get; set; }
    public double BedTemperature { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}