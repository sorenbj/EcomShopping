namespace EcomShopping.Integration.Abstractions;

public interface IIntegrationProvider
{
    string ProviderName { get; }
    string ProviderType { get; }
    Task<bool> TestConnectionAsync();
}
