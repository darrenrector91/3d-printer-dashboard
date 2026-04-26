using Microsoft.AspNetCore.Mvc;
using PrinterDashboard.Api.Services.Interfaces;

namespace PrinterDashboard.Api.Controllers;

[ApiController]
[Route("api/printer")]
public sealed class PrinterController : ControllerBase
{
    [HttpGet("test-mqtt")]
    public async Task<IActionResult> TestMqtt(
        [FromServices] IBambuMqttService bambuMqttService,
        CancellationToken cancellationToken)
    {
        var result = await bambuMqttService.TryConnectAsync(cancellationToken);

        return Ok(new
        {
            success = result.Success,
            message = result.Message
        });
    }

    [HttpGet("mqtt-status")]
    public IActionResult GetMqttStatus(
        [FromServices] IPrinterMqttClientService mqttClientService)
    {
        return Ok(new
        {
            connected = mqttClientService.IsConnected,
            lastMessageAtUtc = mqttClientService.LastMessageAtUtc,
            lastMessageTopic = mqttClientService.LastMessageTopic,
            lastError = mqttClientService.LastError
        });
    }
}