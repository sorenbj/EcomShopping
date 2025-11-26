namespace EcomShopping.Domain.Entities;

public class IntegrationSchedule
{
    public int Id { get; set; }
    public int IntegrationConfigurationId { get; set; }
    public required string ScheduleType { get; set; }
    public string? CronExpression { get; set; }
    public int? IntervalMinutes { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public IntegrationConfiguration? IntegrationConfiguration { get; set; }
}
