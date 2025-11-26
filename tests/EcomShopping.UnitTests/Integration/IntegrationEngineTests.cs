using EcomShopping.Integration.Core;
using EcomShopping.Integration.Core.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EcomShopping.UnitTests.Integration;

public class IntegrationEngineTests
{
    private readonly IntegrationProviderRegistry _registry;
    private readonly IntegrationEngine _engine;

    public IntegrationEngineTests()
    {
        _registry = new IntegrationProviderRegistry();
        _engine = new IntegrationEngine(_registry, NullLogger<IntegrationEngine>.Instance);
        
        // Register mock providers
        _registry.Register("mock-erp", new MockErpIntegration());
        _registry.Register("mock-crm", new MockCrmIntegration());
        _registry.Register("mock-shipping", new MockShippingProvider());
        _registry.Register("mock-payment", new MockPaymentProvider());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidErpProvider_ShouldSucceed()
    {
        // Act
        var result = await _engine.ExecuteAsync("mock-erp", "getproduct", "SKU123");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCrmProvider_ShouldSucceed()
    {
        // Act
        var result = await _engine.ExecuteAsync("mock-crm", "getcustomer", "USER123");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidShippingProvider_ShouldSucceed()
    {
        // Act
        var result = await _engine.ExecuteAsync("mock-shipping", "track", "TRACK123");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithValidPaymentProvider_ShouldSucceed()
    {
        // Act
        var result = await _engine.ExecuteAsync("mock-payment", "getstatus", "TXN123");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidProvider_ShouldReturnFailure()
    {
        // Act
        var result = await _engine.ExecuteAsync("invalid-provider", "test", null);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidOperation_ShouldReturnFailure()
    {
        // Act
        var result = await _engine.ExecuteAsync("mock-erp", "invalid-operation", null);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Unknown");
    }
}
