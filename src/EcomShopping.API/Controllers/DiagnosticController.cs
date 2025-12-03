using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// Diagnostic endpoints for troubleshooting
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    private readonly ILogger<DiagnosticController> _logger;

    public DiagnosticController(ILogger<DiagnosticController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simple ping endpoint
    /// </summary>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint called");
        return Ok(new { message = "pong", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Test endpoint with delay
    /// </summary>
    [HttpGet("delay/{seconds}")]
    public async Task<IActionResult> Delay(int seconds)
    {
        _logger.LogInformation("Delay endpoint called with {Seconds} seconds", seconds);
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        _logger.LogInformation("Delay completed");
        return Ok(new { delayed = seconds, timestamp = DateTime.UtcNow });
    }
}
