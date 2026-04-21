using Microsoft.Extensions.Options;
using PrinterDashboard.Api.Configuration;

namespace PrinterDashboard.Api.Services;

public class PrinterConnectionService
{
    private readonly BambuOptions _options;

    public PrinterConnectionService(IOptions<BambuOptions> options)
    {
        _options = options.Value;
    }

    public object GetConnectionStatus()
    {
        var hostConfigured = !string.IsNullOrWhiteSpace(_options.Host);
        var accessCodeConfigured = !string.IsNullOrWhiteSpace(_options.AccessCode);

        return new
        {
            hostConfigured,
            accessCodeConfigured,
            host = hostConfigured ? _options.Host : null,
            readyToConnect = hostConfigured && accessCodeConfigured
        };
    }
}