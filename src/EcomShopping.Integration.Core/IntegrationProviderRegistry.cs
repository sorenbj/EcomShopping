using EcomShopping.Integration.Abstractions;

namespace EcomShopping.Integration.Core;

public class IntegrationProviderRegistry
{
    private readonly Dictionary<string, IIntegrationProvider> _providers = new();
    private readonly object _lock = new();

    public void Register(string key, IIntegrationProvider provider)
    {
        lock (_lock)
        {
            _providers[key] = provider;
        }
    }

    public T? GetProvider<T>(string key) where T : class, IIntegrationProvider
    {
        lock (_lock)
        {
            if (_providers.TryGetValue(key, out var provider) && provider is T typedProvider)
            {
                return typedProvider;
            }
            return null;
        }
    }

    public IEnumerable<T> GetAllProviders<T>() where T : class, IIntegrationProvider
    {
        lock (_lock)
        {
            return _providers.Values.OfType<T>().ToList();
        }
    }

    public IEnumerable<IIntegrationProvider> GetAllProviders()
    {
        lock (_lock)
        {
            return _providers.Values.ToList();
        }
    }

    public bool Unregister(string key)
    {
        lock (_lock)
        {
            return _providers.Remove(key);
        }
    }

    public bool ContainsProvider(string key)
    {
        lock (_lock)
        {
            return _providers.ContainsKey(key);
        }
    }
}
