namespace EcomShopping.Integration.Core.Configuration;

public class IntegrationSettings
{
    public Dictionary<string, ProviderConfiguration> Providers { get; set; } = new();
}

public class ProviderConfiguration
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string ProviderClass { get; set; }
    public bool Enabled { get; set; }
    public Dictionary<string, string> Settings { get; set; } = new();
}

public class SecureCredential
{
    public required string Key { get; set; }
    public required string EncryptedValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
