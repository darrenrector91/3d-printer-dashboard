namespace PrinterDashboard.Api.Models;

public sealed class PrinterStatus
{
    public PrinterState State { get; set; }
    public int ProgressPercent { get; set; }
    public int CurrentLayer { get; set; }
    public int TotalLayers { get; set; }
    public int RemainingMinutes { get; set; }
    public double NozzleTemperature { get; set; }
    public double BedTemperature { get; set; }
    public double ChamberTemperature { get; set; }
    public int PrintErrorCode { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
}