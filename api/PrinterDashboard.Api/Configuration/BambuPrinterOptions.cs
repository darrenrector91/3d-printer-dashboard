namespace PrinterDashboard.Api.Configuration;

public class BambuPrinterOptions
{
    public const string SectionName = "BambuPrinter";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 8883;

    public string Username { get; set; } = "bblp";

    public string AccessCode { get; set; } = string.Empty;
}