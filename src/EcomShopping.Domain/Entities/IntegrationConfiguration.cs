namespace EcomShopping.Domain.Entities;

public class IntegrationConfiguration
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string ProviderType { get; set; }
    public required string ProviderKey { get; set; }
    public required string ConfigurationJson { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}
