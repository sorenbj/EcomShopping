namespace EcomShopping.Domain.Entities;

public class IntegrationExecution
{
    public int Id { get; set; }
    public int IntegrationConfigurationId { get; set; }
    public required string ExecutionType { get; set; }
    public required string Status { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResultJson { get; set; }
    public string? TriggeredBy { get; set; }
    
    public IntegrationConfiguration? IntegrationConfiguration { get; set; }
}
