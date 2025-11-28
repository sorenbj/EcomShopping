using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class CategoryRepository : IRepository<Category>
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.Products)
            .ToListAsync();
    }

    public async Task<Category> AddAsync(Category entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Categories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Category entity)
    {
        var existingCategory = await _context.Categories.FindAsync(entity.Id);
        if (existingCategory == null)
        {
            throw new InvalidOperationException($"Category with ID {entity.Id} not found.");
        }

        // Update only the scalar properties
        existingCategory.Name = entity.Name;
        existingCategory.Description = entity.Description;
        existingCategory.ParentCategoryId = entity.ParentCategoryId;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
