using EcomShopping.Domain.Entities;
using EcomShopping.Infrastructure.Data;
using EcomShopping.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace EcomShopping.IntegrationTests.Repositories;

public class LowStockEventRepositoryTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateEventAsync_ShouldCreateLowStockEvent()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var lowStockEvent = await repository.CreateEventAsync(product.Id, 5, 10);

        // Assert
        lowStockEvent.Should().NotBeNull();
        lowStockEvent.ProductId.Should().Be(product.Id);
        lowStockEvent.ProductName.Should().Be("Test Product");
        lowStockEvent.ProductSKU.Should().Be("TEST-001");
        lowStockEvent.CurrentStock.Should().Be(5);
        lowStockEvent.Threshold.Should().Be(10);
        lowStockEvent.IsAcknowledged.Should().BeFalse();
    }

    [Fact]
    public async Task GetUnacknowledgedAsync_ShouldReturnOnlyUnacknowledged()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var event1 = await repository.CreateEventAsync(product.Id, 5, 10);
        var event2 = await repository.CreateEventAsync(product.Id, 3, 10);
        await repository.AcknowledgeEventAsync(event1.Id, "admin");

        // Act
        var unacknowledged = await repository.GetUnacknowledgedAsync();

        // Assert
        unacknowledged.Should().HaveCount(1);
        unacknowledged.First().Id.Should().Be(event2.Id);
    }

    [Fact]
    public async Task AcknowledgeEventAsync_ShouldMarkAsAcknowledged()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var lowStockEvent = await repository.CreateEventAsync(product.Id, 5, 10);

        // Act
        await repository.AcknowledgeEventAsync(lowStockEvent.Id, "admin");

        // Assert
        var acknowledged = await context.LowStockEvents.FindAsync(lowStockEvent.Id);
        acknowledged.Should().NotBeNull();
        acknowledged!.IsAcknowledged.Should().BeTrue();
        acknowledged.AcknowledgedBy.Should().Be("admin");
        acknowledged.AcknowledgedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HasRecentEventAsync_WithRecentEvent_ShouldReturnTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        await repository.CreateEventAsync(product.Id, 5, 10);

        // Act
        var hasRecent = await repository.HasRecentEventAsync(product.Id, 24);

        // Assert
        hasRecent.Should().BeTrue();
    }

    [Fact]
    public async Task HasRecentEventAsync_WithNoRecentEvent_ShouldReturnFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Create an old event
        var oldEvent = new LowStockEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSKU = product.SKU,
            CurrentStock = 5,
            Threshold = 10,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsAcknowledged = false
        };
        context.LowStockEvents.Add(oldEvent);
        await context.SaveChangesAsync();

        // Act
        var hasRecent = await repository.HasRecentEventAsync(product.Id, 24);

        // Assert
        hasRecent.Should().BeFalse();
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnAllEventsForProduct()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new LowStockEventRepository(context);

        var product1 = new Product
        {
            Name = "Test Product 1",
            Slug = "test-product-1",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true
        };
        var product2 = new Product
        {
            Name = "Test Product 2",
            Slug = "test-product-2",
            SKU = "TEST-002",
            Price = 99.99m,
            StockQuantity = 3,
            LowStockThreshold = 10,
            IsActive = true
        };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        await repository.CreateEventAsync(product1.Id, 5, 10);
        await repository.CreateEventAsync(product1.Id, 3, 10);
        await repository.CreateEventAsync(product2.Id, 3, 10);

        // Act
        var events = await repository.GetByProductIdAsync(product1.Id);

        // Assert
        events.Should().HaveCount(2);
        events.Should().AllSatisfy(e => e.ProductId.Should().Be(product1.Id));
    }
}
