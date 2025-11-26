using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using EcomShopping.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace EcomShopping.IntegrationTests.Repositories;

public class StockMovementRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IStockMovementRepository _stockRepository;
    private readonly IProductRepository _productRepository;

    public StockMovementRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _stockRepository = new StockMovementRepository(_context);
        _productRepository = new ProductRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddMovementAsync_Purchase_ShouldIncreaseStock()
    {
        // Arrange
        var product = await _productRepository.AddAsync(new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 10,
            IsActive = true
        });

        // Act
        var movement = await _stockRepository.AddMovementAsync(
            product.Id,
            50,
            StockMovementType.Purchase,
            "PO-12345",
            "Restocking"
        );

        // Assert
        movement.Should().NotBeNull();
        movement.Quantity.Should().Be(50);
        movement.Type.Should().Be(StockMovementType.Purchase);
        
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(60);
    }

    [Fact]
    public async Task AddMovementAsync_Sale_ShouldDecreaseStock()
    {
        // Arrange
        var product = await _productRepository.AddAsync(new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        });

        // Act
        var movement = await _stockRepository.AddMovementAsync(
            product.Id,
            25,
            StockMovementType.Sale,
            "ORDER-123"
        );

        // Assert
        movement.Should().NotBeNull();
        movement.Type.Should().Be(StockMovementType.Sale);
        
        var updatedProduct = await _productRepository.GetByIdAsync(product.Id);
        updatedProduct!.StockQuantity.Should().Be(75);
    }

    [Fact]
    public async Task GetByProductIdAsync_ShouldReturnMovements()
    {
        // Arrange
        var product = await _productRepository.AddAsync(new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 100,
            IsActive = true
        });

        await _stockRepository.AddMovementAsync(product.Id, 50, StockMovementType.Purchase);
        await _stockRepository.AddMovementAsync(product.Id, 10, StockMovementType.Sale);
        await _stockRepository.AddMovementAsync(product.Id, 5, StockMovementType.Damage);

        // Act
        var movements = await _stockRepository.GetByProductIdAsync(product.Id);

        // Assert
        movements.Should().HaveCount(3);
    }
}
