using EcomShopping.Integration.Abstractions;
using EcomShopping.Integration.Core;
using EcomShopping.Integration.Core.Providers;
using FluentAssertions;

namespace EcomShopping.UnitTests.Integration;

public class IntegrationProviderRegistryTests
{
    [Fact]
    public void Register_ShouldAddProviderToRegistry()
    {
        // Arrange
        var registry = new IntegrationProviderRegistry();
        var provider = new MockErpIntegration();

        // Act
        registry.Register("test-erp", provider);

        // Assert
        registry.ContainsProvider("test-erp").Should().BeTrue();
    }

    [Fact]
    public void GetProvider_WithValidKey_ShouldReturnProvider()
    {
        // Arrange
        var registry = new IntegrationProviderRegistry();
        var provider = new MockErpIntegration();
        registry.Register("test-erp", provider);

        // Act
        var result = registry.GetProvider<IErpIntegration>("test-erp");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<MockErpIntegration>();
    }

    [Fact]
    public void GetProvider_WithInvalidKey_ShouldReturnNull()
    {
        // Arrange
        var registry = new IntegrationProviderRegistry();

        // Act
        var result = registry.GetProvider<IErpIntegration>("invalid-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllProviders_ShouldReturnAllRegisteredProviders()
    {
        // Arrange
        var registry = new IntegrationProviderRegistry();
        registry.Register("erp1", new MockErpIntegration());
        registry.Register("erp2", new MockErpIntegration());
        registry.Register("crm1", new MockCrmIntegration());

        // Act
        var erpProviders = registry.GetAllProviders<IErpIntegration>();
        var allProviders = registry.GetAllProviders();

        // Assert
        erpProviders.Should().HaveCount(2);
        allProviders.Should().HaveCount(3);
    }

    [Fact]
    public void Unregister_ShouldRemoveProvider()
    {
        // Arrange
        var registry = new IntegrationProviderRegistry();
        registry.Register("test-erp", new MockErpIntegration());

        // Act
        var removed = registry.Unregister("test-erp");

        // Assert
        removed.Should().BeTrue();
        registry.ContainsProvider("test-erp").Should().BeFalse();
    }
}
