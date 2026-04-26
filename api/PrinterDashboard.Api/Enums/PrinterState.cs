namespace PrinterDashboard.Api.Models;

public enum PrinterState
{
    Unknown = 0,
    Idle = 1,
    Printing = 2,
    Paused = 3,
    Error = 4
}