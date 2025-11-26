using EcomShopping.Integration.Core;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController : ControllerBase
{
    private readonly IntegrationEngine _integrationEngine;
    private readonly IntegrationProviderRegistry _providerRegistry;
    private readonly IntegrationScheduler _scheduler;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IntegrationEngine integrationEngine,
        IntegrationProviderRegistry providerRegistry,
        IntegrationScheduler scheduler,
        ILogger<IntegrationsController> logger)
    {
        _integrationEngine = integrationEngine;
        _providerRegistry = providerRegistry;
        _scheduler = scheduler;
        _logger = logger;
    }

    /// <summary>
    /// Get all registered integration providers
    /// </summary>
    [HttpGet("providers")]
    public ActionResult<object> GetProviders()
    {
        try
        {
            var providers = _providerRegistry.GetAllProviders()
                .Select(p => new
                {
                    p.ProviderName,
                    p.ProviderType
                });
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting providers");
            return StatusCode(500, new { error = "Error retrieving providers" });
        }
    }

    /// <summary>
    /// Test connection to a specific provider
    /// </summary>
    [HttpGet("providers/{providerKey}/test")]
    public async Task<ActionResult<object>> TestConnection(string providerKey)
    {
        try
        {
            var provider = _providerRegistry.GetProvider<Integration.Abstractions.IIntegrationProvider>(providerKey);
            if (provider == null)
            {
                return NotFound(new { error = $"Provider '{providerKey}' not found" });
            }

            var result = await provider.TestConnectionAsync();
            return Ok(new { success = result, providerName = provider.ProviderName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection for provider: {ProviderKey}", providerKey);
            return StatusCode(500, new { error = "Error testing connection" });
        }
    }

    /// <summary>
    /// Execute an integration operation
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult<object>> ExecuteIntegration([FromBody] ExecuteIntegrationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProviderKey) || string.IsNullOrEmpty(request.Operation))
            {
                return BadRequest(new { error = "ProviderKey and Operation are required" });
            }

            var result = await _integrationEngine.ExecuteAsync(request.ProviderKey, request.Operation, request.Parameters);
            
            if (result.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing integration");
            return StatusCode(500, new { error = "Error executing integration" });
        }
    }

    /// <summary>
    /// Add a scheduled integration
    /// </summary>
    [HttpPost("schedules")]
    public ActionResult<object> AddSchedule([FromBody] AddScheduleRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ProviderKey) || string.IsNullOrEmpty(request.Operation))
            {
                return BadRequest(new { error = "ProviderKey and Operation are required" });
            }

            if (request.IntervalMinutes <= 0)
            {
                return BadRequest(new { error = "IntervalMinutes must be greater than 0" });
            }

            _scheduler.AddSchedule(
                request.ScheduleId,
                request.ProviderKey,
                request.Operation,
                request.IntervalMinutes,
                request.Parameters);

            return Ok(new { success = true, message = "Schedule added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding schedule");
            return StatusCode(500, new { error = "Error adding schedule" });
        }
    }

    /// <summary>
    /// Get all schedules
    /// </summary>
    [HttpGet("schedules")]
    public ActionResult<object> GetSchedules()
    {
        try
        {
            var schedules = _scheduler.GetSchedules();
            return Ok(schedules);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedules");
            return StatusCode(500, new { error = "Error retrieving schedules" });
        }
    }

    /// <summary>
    /// Remove a schedule
    /// </summary>
    [HttpDelete("schedules/{scheduleId}")]
    public ActionResult<object> RemoveSchedule(int scheduleId)
    {
        try
        {
            _scheduler.RemoveSchedule(scheduleId);
            return Ok(new { success = true, message = "Schedule removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing schedule");
            return StatusCode(500, new { error = "Error removing schedule" });
        }
    }

    /// <summary>
    /// Manually trigger execution of due schedules
    /// </summary>
    [HttpPost("schedules/execute")]
    public async Task<ActionResult<object>> ExecuteSchedules()
    {
        try
        {
            var results = await _scheduler.ExecuteDueSchedulesAsync();
            return Ok(new
            {
                success = true,
                executedCount = results.Count,
                results
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing schedules");
            return StatusCode(500, new { error = "Error executing schedules" });
        }
    }
}

public class ExecuteIntegrationRequest
{
    public string ProviderKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}

public class AddScheduleRequest
{
    public int ScheduleId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; }
    public object? Parameters { get; set; }
}
