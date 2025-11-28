using EcomShopping.Infrastructure.Services;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using FluentAssertions;

namespace EcomShopping.UnitTests.Services;

public class InventoryServiceTests
{
    private readonly Mock<IStockReservationRepository> _stockReservationRepositoryMock;
    private readonly Mock<ILowStockEventRepository> _lowStockEventRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ILogger<InventoryService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly InventoryService _inventoryService;

    public InventoryServiceTests()
    {
        _stockReservationRepositoryMock = new Mock<IStockReservationRepository>();
        _lowStockEventRepositoryMock = new Mock<ILowStockEventRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<InventoryService>>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup configuration mock to return default value
        _configurationMock.Setup(x => x["Inventory:ReservationExpirationMinutes"]).Returns("15");

        _inventoryService = new InventoryService(
            _stockReservationRepositoryMock.Object,
            _lowStockEventRepositoryMock.Object,
            _productRepositoryMock.Object,
            _loggerMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task ReserveCartStockAsync_WithSufficientStock_ShouldReserveSuccessfully()
    {
        // Arrange
        var cartItems = new List<(int ProductId, int Quantity)>
        {
            (1, 5),
            (2, 3)
        };
        var sessionId = "test-session-123";

        _stockReservationRepositoryMock
            .Setup(x => x.ReserveStockAsync(It.IsAny<int>(), It.IsAny<int>(), sessionId, 15))
            .ReturnsAsync((int productId, int quantity, string sid, int exp) => new StockReservation
            {
                Id = productId,
                ProductId = productId,
                Quantity = quantity,
                SessionId = sid
            });

        // Act
        var reservationIds = await _inventoryService.ReserveCartStockAsync(cartItems, sessionId);

        // Assert
        reservationIds.Should().HaveCount(2);
        _stockReservationRepositoryMock.Verify(x => x.ReserveStockAsync(1, 5, sessionId, 15), Times.Once);
        _stockReservationRepositoryMock.Verify(x => x.ReserveStockAsync(2, 3, sessionId, 15), Times.Once);
    }

    [Fact]
    public async Task ReserveCartStockAsync_WithInsufficientStock_ShouldReleaseReservationsAndThrow()
    {
        // Arrange
        var cartItems = new List<(int ProductId, int Quantity)>
        {
            (1, 5),
            (2, 3)
        };
        var sessionId = "test-session-123";

        _stockReservationRepositoryMock
            .SetupSequence(x => x.ReserveStockAsync(It.IsAny<int>(), It.IsAny<int>(), sessionId, 15))
            .ReturnsAsync(new StockReservation { Id = 1, ProductId = 1 })
            .ThrowsAsync(new InvalidOperationException("Insufficient stock"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _inventoryService.ReserveCartStockAsync(cartItems, sessionId));

        _stockReservationRepositoryMock.Verify(x => x.ReleaseReservationAsync(1), Times.Once);
    }

    [Fact]
    public async Task ReleaseSessionReservationsAsync_ShouldCallRepository()
    {
        // Arrange
        var sessionId = "test-session-123";

        // Act
        await _inventoryService.ReleaseSessionReservationsAsync(sessionId);

        // Assert
        _stockReservationRepositoryMock.Verify(x => x.ReleaseSessionReservationsAsync(sessionId), Times.Once);
    }

    [Fact]
    public async Task CheckLowStockLevelsAsync_WithLowStock_ShouldCreateEvent()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", SKU = "SKU1", StockQuantity = 5, LowStockThreshold = 10, IsActive = true },
            new Product { Id = 2, Name = "Product 2", SKU = "SKU2", StockQuantity = 50, LowStockThreshold = 10, IsActive = true }
        };

        var availableStockMap = new Dictionary<int, int>
        {
            { 1, 5 },
            { 2, 50 }
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(products[0]);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(products[1]);
        _stockReservationRepositoryMock.Setup(x => x.GetAvailableStockBatchAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(availableStockMap);
        _lowStockEventRepositoryMock.Setup(x => x.HasRecentEventAsync(It.IsAny<int>(), 24)).ReturnsAsync(false);

        // Act
        await _inventoryService.CheckLowStockLevelsAsync();

        // Assert
        _lowStockEventRepositoryMock.Verify(x => x.CreateEventAsync(1, 5, 10), Times.Once);
        _lowStockEventRepositoryMock.Verify(x => x.CreateEventAsync(2, It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CheckLowStockLevelsAsync_WithRecentEvent_ShouldNotCreateDuplicate()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", SKU = "SKU1", StockQuantity = 5, LowStockThreshold = 10, IsActive = true }
        };

        var availableStockMap = new Dictionary<int, int>
        {
            { 1, 5 }
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
        _stockReservationRepositoryMock.Setup(x => x.GetAvailableStockBatchAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(availableStockMap);
        _lowStockEventRepositoryMock.Setup(x => x.HasRecentEventAsync(1, 24)).ReturnsAsync(true);

        // Act
        await _inventoryService.CheckLowStockLevelsAsync();

        // Assert
        _lowStockEventRepositoryMock.Verify(x => x.CreateEventAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAvailableStockAsync_ShouldReturnFromRepository()
    {
        // Arrange
        var productId = 1;
        var expectedStock = 45;

        _stockReservationRepositoryMock.Setup(x => x.GetAvailableStockAsync(productId)).ReturnsAsync(expectedStock);

        // Act
        var result = await _inventoryService.GetAvailableStockAsync(productId);

        // Assert
        result.Should().Be(expectedStock);
        _stockReservationRepositoryMock.Verify(x => x.GetAvailableStockAsync(productId), Times.Once);
    }

    [Fact]
    public async Task CheckLowStockLevelsAsync_WithInactiveProducts_ShouldSkipThem()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", SKU = "SKU1", StockQuantity = 5, LowStockThreshold = 10, IsActive = true },
            new Product { Id = 2, Name = "Product 2", SKU = "SKU2", StockQuantity = 3, LowStockThreshold = 10, IsActive = false }
        };

        var availableStockMap = new Dictionary<int, int>
        {
            { 1, 5 }
        };

        _productRepositoryMock.Setup(x => x.GetAllAsync()).ReturnsAsync(products);
        _productRepositoryMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(products[0]);
        _stockReservationRepositoryMock.Setup(x => x.GetAvailableStockBatchAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(availableStockMap);
        _lowStockEventRepositoryMock.Setup(x => x.HasRecentEventAsync(1, 24)).ReturnsAsync(false);

        // Act
        await _inventoryService.CheckLowStockLevelsAsync();

        // Assert
        // Should only call batch method with active product IDs
        _stockReservationRepositoryMock.Verify(x => x.GetAvailableStockBatchAsync(
            It.Is<IEnumerable<int>>(ids => ids.Count() == 1 && ids.Contains(1))), Times.Once);
        // Should only create event for active product 1
        _lowStockEventRepositoryMock.Verify(x => x.CreateEventAsync(1, 5, 10), Times.Once);
    }
}
