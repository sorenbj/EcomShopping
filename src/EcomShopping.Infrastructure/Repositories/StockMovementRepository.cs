using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly ApplicationDbContext _context;

    public StockMovementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovement?> GetByIdAsync(int id)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .FirstOrDefaultAsync(sm => sm.Id == id);
    }

    public async Task<IEnumerable<StockMovement>> GetAllAsync()
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
    {
        return await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .ToListAsync();
    }

    public async Task<StockMovement> AddMovementAsync(int productId, int quantity, StockMovementType type, string? reference = null, string? notes = null, string? createdBy = null)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        var movement = new StockMovement
        {
            ProductId = productId,
            Quantity = quantity,
            Type = type,
            Reference = reference,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Store original stock for error message
        var originalStock = product.StockQuantity;

        // Update product stock quantity based on movement type
        // Purchase, Return, and positive Adjustments increase stock
        // Sale, Damage, and negative Adjustments decrease stock
        switch (type)
        {
            case StockMovementType.Purchase:
            case StockMovementType.Return:
                product.StockQuantity += quantity;
                break;
            case StockMovementType.Sale:
            case StockMovementType.Damage:
                product.StockQuantity -= quantity;
                break;
            case StockMovementType.Adjustment:
                // For adjustments, quantity can be positive (increase) or negative (decrease)
                product.StockQuantity += quantity;
                break;
        }

        // Ensure stock doesn't go negative
        if (product.StockQuantity < 0)
        {
            throw new InvalidOperationException($"Insufficient stock. Current stock: {originalStock}, requested: {Math.Abs(quantity)}, would result in: {product.StockQuantity}");
        }

        product.UpdatedAt = DateTime.UtcNow;

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        return movement;
    }

    public async Task<StockMovement> AddAsync(StockMovement entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.StockMovements.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StockMovement entity)
    {
        _context.StockMovements.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var movement = await _context.StockMovements.FindAsync(id);
        if (movement != null)
        {
            _context.StockMovements.Remove(movement);
            await _context.SaveChangesAsync();
        }
    }
}
