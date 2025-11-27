using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? searchTerm = null, int? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<Product> AddAsync(Product entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Product entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }
}
