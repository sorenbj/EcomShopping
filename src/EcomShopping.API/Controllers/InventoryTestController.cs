using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// Test controller with minimal dependencies
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryTestController : ControllerBase
{
    private readonly ILogger<InventoryTestController> _logger;

    public InventoryTestController(ILogger<InventoryTestController> _logger)
    {
        _logger.LogInformation("InventoryTestController constructor called");
        this._logger = _logger;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        _logger.LogInformation("Test endpoint called");
        return Ok(new { message = "Test endpoint working!", timestamp = DateTime.UtcNow });
    }
}
