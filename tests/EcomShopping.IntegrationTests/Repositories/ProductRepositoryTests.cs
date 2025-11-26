using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using EcomShopping.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace EcomShopping.IntegrationTests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProductRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddAsync_ShouldCreateProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "Test Product",
            Slug = "test-product",
            SKU = "TEST-001",
            Price = 99.99m,
            StockQuantity = 10,
            IsActive = true,
            Description = "Test description",
            Metadata = new Dictionary<string, string> { { "color", "blue" } }
        };

        // Act
        var result = await _repository.AddAsync(product);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "Laptop",
            Slug = "gaming-laptop",
            SKU = "LAP-001",
            Price = 1299.99m,
            IsActive = true
        };
        await _repository.AddAsync(product);

        // Act
        var result = await _repository.GetBySlugAsync("gaming-laptop");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Laptop");
        result.Slug.Should().Be("gaming-laptop");
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        for (int i = 1; i <= 15; i++)
        {
            await _repository.AddAsync(new Product
            {
                Name = $"Product {i}",
                Slug = $"product-{i}",
                SKU = $"SKU-{i:000}",
                Price = i * 10,
                IsActive = true
            });
        }

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(page: 2, pageSize: 5);

        // Assert
        totalCount.Should().Be(15);
        items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearch_ShouldFilterResults()
    {
        // Arrange
        await _repository.AddAsync(new Product
        {
            Name = "Laptop Computer",
            Slug = "laptop",
            SKU = "LAP-001",
            Price = 999,
            IsActive = true
        });
        await _repository.AddAsync(new Product
        {
            Name = "Desktop Computer",
            Slug = "desktop",
            SKU = "DES-001",
            Price = 799,
            IsActive = true
        });

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(page: 1, pageSize: 10, searchTerm: "Laptop");

        // Assert
        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items.First().Name.Should().Be("Laptop Computer");
    }
}
