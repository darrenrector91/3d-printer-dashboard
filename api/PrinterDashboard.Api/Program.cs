var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/ping", () => Results.Ok(new
{
    message = "PrinterDashboard API is running"
}));

app.MapGet("/api/printer/config-status", (IConfiguration config) =>
{
    var host = config["Bambu:Host"];
    var accessCode = config["Bambu:AccessCode"];

    return Results.Ok(new
    {
        hostConfigured = !string.IsNullOrWhiteSpace(host),
        accessCodeConfigured = !string.IsNullOrWhiteSpace(accessCode)
    });
});

app.MapGet("/api/printer/target", (IConfiguration config) =>
{
    var host = config["Bambu:Host"];

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