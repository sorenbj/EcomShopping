using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using EcomShopping.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace EcomShopping.IntegrationTests.Repositories;

public class CategoryRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Category> _repository;

    public CategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new CategoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeProductsCollection()
    {
        // Arrange
        var category = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices and gadgets",
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(category);

        var product1 = new Product
        {
            Name = "Laptop",
            Slug = "laptop",
            SKU = "LAP-001",
            Price = 1299.99m,
            IsActive = true,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };
        var product2 = new Product
        {
            Name = "Mouse",
            Slug = "mouse",
            SKU = "MOU-001",
            Price = 29.99m,
            IsActive = true,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product1);
        _context.Products.Add(product2);
        await _context.SaveChangesAsync();

        // Act
        var categories = await _repository.GetAllAsync();
        var result = categories.FirstOrDefault(c => c.Id == category.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Products.Should().NotBeNull();
        result.Products.Should().HaveCount(2);
        result.Products.Should().Contain(p => p.Name == "Laptop");
        result.Products.Should().Contain(p => p.Name == "Mouse");
    }

    [Fact]
    public async Task GetAllAsync_ShouldIncludeParentCategory()
    {
        // Arrange
        var parentCategory = new Category
        {
            Name = "Electronics",
            Description = "Electronic devices",
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(parentCategory);

        var childCategory = new Category
        {
            Name = "Laptops",
            Description = "Laptop computers",
            ParentCategoryId = parentCategory.Id,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(childCategory);

        // Act
        var categories = await _repository.GetAllAsync();
        var result = categories.FirstOrDefault(c => c.Id == childCategory.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ParentCategory.Should().NotBeNull();
        result.ParentCategory!.Name.Should().Be("Electronics");
    }

    [Fact]
    public async Task AddAsync_ShouldCreateCategory()
    {
        // Arrange
        var category = new Category
        {
            Name = "Books",
            Description = "Books and literature",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(category);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
