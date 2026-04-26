using PrinterDashboard.Api.Configuration;
using PrinterDashboard.Api.Services;
using PrinterDashboard.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BambuPrinterOptions>(
    builder.Configuration.GetSection(BambuPrinterOptions.SectionName));

builder.Services.AddSingleton<PrinterConnectionService>();
builder.Services.AddScoped<IBambuMqttService, BambuMqttService>();

builder.Services.AddSingleton<PrinterMqttHostedService>();
builder.Services.AddSingleton<IPrinterMqttClientService>(sp => sp.GetRequiredService<PrinterMqttHostedService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<PrinterMqttHostedService>());

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/api/ping", () => Results.Ok(new
{
    message = "PrinterDashboard API is running"
}));

app.MapGet("/api/printer/config-status", (PrinterConnectionService service) =>
{
    return Results.Ok(service.GetConnectionStatus());
});

app.MapGet("/api/printer/target", (IConfiguration config) =>
{
    var host = config["BambuPrinter:Host"];

    if (string.IsNullOrWhiteSpace(host))
    {
        return Results.BadRequest(new
        {
            message = "Printer host is not configured"
        });
    }

    return Results.Ok(new
    {
        host
    });
});

app.Run();