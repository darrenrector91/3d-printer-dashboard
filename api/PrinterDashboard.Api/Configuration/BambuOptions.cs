namespace PrinterDashboard.Api.Configuration;

public class BambuOptions
{
    public const string SectionName = "Bambu";
    public string Host { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
}