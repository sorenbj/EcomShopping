using EcomShopping.Domain.Entities;
using EcomShopping.Infrastructure.Data;
using EcomShopping.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace EcomShopping.IntegrationTests.Repositories;

public class StockReservationRepositoryTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ReserveStockAsync_WithSufficientStock_ShouldCreateReservation()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var reservation = await repository.ReserveStockAsync(product.Id, 10, "session-123", 15);

        // Assert
        reservation.Should().NotBeNull();
        reservation.ProductId.Should().Be(product.Id);
        reservation.Quantity.Should().Be(10);
        reservation.SessionId.Should().Be("session-123");
        reservation.IsReleased.Should().BeFalse();
        reservation.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task ReserveStockAsync_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 5,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.ReserveStockAsync(product.Id, 10, "session-123", 15));
    }

    [Fact]
    public async Task GetAvailableStockAsync_WithNoReservations_ShouldReturnFullStock()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Act
        var availableStock = await repository.GetAvailableStockAsync(product.Id);

        // Assert
        availableStock.Should().Be(100);
    }

    [Fact]
    public async Task GetAvailableStockAsync_WithActiveReservations_ShouldReturnReducedStock()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Create reservations
        await repository.ReserveStockAsync(product.Id, 10, "session-1", 15);
        await repository.ReserveStockAsync(product.Id, 5, "session-2", 15);

        // Act
        var availableStock = await repository.GetAvailableStockAsync(product.Id);

        // Assert
        availableStock.Should().Be(85); // 100 - 10 - 5
    }

    [Fact]
    public async Task ReleaseReservationAsync_ShouldMarkAsReleased()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var reservation = await repository.ReserveStockAsync(product.Id, 10, "session-123", 15);

        // Act
        await repository.ReleaseReservationAsync(reservation.Id);

        // Assert
        var released = await context.StockReservations.FindAsync(reservation.Id);
        released.Should().NotBeNull();
        released!.IsReleased.Should().BeTrue();
        released.ReleasedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ReleaseSessionReservationsAsync_ShouldReleaseAllSessionReservations()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var reservation1 = await repository.ReserveStockAsync(product.Id, 10, "session-123", 15);
        var reservation2 = await repository.ReserveStockAsync(product.Id, 5, "session-123", 15);
        var reservation3 = await repository.ReserveStockAsync(product.Id, 3, "session-456", 15);

        // Act
        await repository.ReleaseSessionReservationsAsync("session-123");

        // Assert
        var released1 = await context.StockReservations.FindAsync(reservation1.Id);
        var released2 = await context.StockReservations.FindAsync(reservation2.Id);
        var notReleased = await context.StockReservations.FindAsync(reservation3.Id);

        released1!.IsReleased.Should().BeTrue();
        released2!.IsReleased.Should().BeTrue();
        notReleased!.IsReleased.Should().BeFalse();
    }

    [Fact]
    public async Task ReleaseExpiredReservationsAsync_ShouldReleaseOnlyExpired()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        // Create an expired reservation
        var expiredReservation = new StockReservation
        {
            ProductId = product.Id,
            Quantity = 10,
            SessionId = "session-expired",
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            IsReleased = false
        };
        context.StockReservations.Add(expiredReservation);
        await context.SaveChangesAsync();

        // Create a valid reservation
        var validReservation = await repository.ReserveStockAsync(product.Id, 5, "session-valid", 15);

        // Act
        await repository.ReleaseExpiredReservationsAsync();

        // Assert
        var expired = await context.StockReservations.FindAsync(expiredReservation.Id);
        var valid = await context.StockReservations.FindAsync(validReservation.Id);

        expired!.IsReleased.Should().BeTrue();
        valid!.IsReleased.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmReservationAsync_ShouldSetOrderNumberAndRelease()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var repository = new StockReservationRepository(context);

        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var reservation = await repository.ReserveStockAsync(product.Id, 10, "session-123", 15);

        // Act
        await repository.ConfirmReservationAsync(reservation.Id, "ORD-123456");

        // Assert
        var confirmed = await context.StockReservations.FindAsync(reservation.Id);
        confirmed.Should().NotBeNull();
        confirmed!.OrderNumber.Should().Be("ORD-123456");
        confirmed.IsReleased.Should().BeTrue();
        confirmed.ReleasedAt.Should().NotBeNull();
    }
}
