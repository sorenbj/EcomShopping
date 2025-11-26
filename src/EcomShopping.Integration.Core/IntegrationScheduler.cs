using Microsoft.Extensions.Logging;

namespace EcomShopping.Integration.Core;

public class IntegrationScheduler
{
    private readonly IntegrationEngine _integrationEngine;
    private readonly ILogger<IntegrationScheduler> _logger;
    private readonly Dictionary<int, ScheduleEntry> _schedules = new();
    private readonly object _lock = new();

    public IntegrationScheduler(IntegrationEngine integrationEngine, ILogger<IntegrationScheduler> logger)
    {
        _integrationEngine = integrationEngine;
        _logger = logger;
    }

    public void AddSchedule(int scheduleId, string providerKey, string operation, int intervalMinutes, object? parameters = null)
    {
        lock (_lock)
        {
            var entry = new ScheduleEntry
            {
                ScheduleId = scheduleId,
                ProviderKey = providerKey,
                Operation = operation,
                IntervalMinutes = intervalMinutes,
                Parameters = parameters,
                NextExecutionTime = DateTime.UtcNow.AddMinutes(intervalMinutes)
            };

            _schedules[scheduleId] = entry;
            _logger.LogInformation("Schedule added: {ScheduleId}, Provider: {ProviderKey}, Interval: {Interval} minutes", 
                scheduleId, providerKey, intervalMinutes);
        }
    }

    public void RemoveSchedule(int scheduleId)
    {
        lock (_lock)
        {
            if (_schedules.Remove(scheduleId))
            {
                _logger.LogInformation("Schedule removed: {ScheduleId}", scheduleId);
            }
        }
    }

    public async Task<List<ScheduleExecutionResult>> ExecuteDueSchedulesAsync()
    {
        var results = new List<ScheduleExecutionResult>();
        var now = DateTime.UtcNow;
        List<ScheduleEntry> dueSchedules;

        lock (_lock)
        {
            dueSchedules = _schedules.Values
                .Where(s => s.NextExecutionTime <= now)
                .ToList();
        }

        foreach (var schedule in dueSchedules)
        {
            try
            {
                _logger.LogInformation("Executing scheduled integration: {ScheduleId}", schedule.ScheduleId);
                
                var result = await _integrationEngine.ExecuteAsync(schedule.ProviderKey, schedule.Operation, schedule.Parameters);
                
                results.Add(new ScheduleExecutionResult
                {
                    ScheduleId = schedule.ScheduleId,
                    Success = result.IsSuccess,
                    Message = result.Message,
                    ExecutedAt = now
                });

                // Update next execution time
                lock (_lock)
                {
                    if (_schedules.TryGetValue(schedule.ScheduleId, out var entry))
                    {
                        entry.LastExecutionTime = now;
                        entry.NextExecutionTime = now.AddMinutes(schedule.IntervalMinutes);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scheduled integration: {ScheduleId}", schedule.ScheduleId);
                
                results.Add(new ScheduleExecutionResult
                {
                    ScheduleId = schedule.ScheduleId,
                    Success = false,
                    Message = $"Error: {ex.Message}",
                    ExecutedAt = now
                });
            }
        }

        return results;
    }

    public IEnumerable<ScheduleInfo> GetSchedules()
    {
        lock (_lock)
        {
            return _schedules.Values.Select(s => new ScheduleInfo
            {
                ScheduleId = s.ScheduleId,
                ProviderKey = s.ProviderKey,
                Operation = s.Operation,
                IntervalMinutes = s.IntervalMinutes,
                LastExecutionTime = s.LastExecutionTime,
                NextExecutionTime = s.NextExecutionTime
            }).ToList();
        }
    }
}

public class ScheduleEntry
{
    public int ScheduleId { get; set; }
    public required string ProviderKey { get; set; }
    public required string Operation { get; set; }
    public int IntervalMinutes { get; set; }
    public object? Parameters { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public DateTime NextExecutionTime { get; set; }
}

public class ScheduleExecutionResult
{
    public int ScheduleId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}

public class ScheduleInfo
{
    public int ScheduleId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public int IntervalMinutes { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public DateTime NextExecutionTime { get; set; }
}
