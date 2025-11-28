using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class LowStockEventRepository : ILowStockEventRepository
{
    private readonly ApplicationDbContext _context;

    public LowStockEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LowStockEvent?> GetByIdAsync(int id)
    {
        return await _context.LowStockEvents
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<LowStockEvent>> GetAllAsync()
    {
        return await _context.LowStockEvents
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LowStockEvent>> GetUnacknowledgedAsync()
    {
        return await _context.LowStockEvents
            .Where(e => !e.IsAcknowledged)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LowStockEvent>> GetByProductIdAsync(int productId)
    {
        return await _context.LowStockEvents
            .Where(e => e.ProductId == productId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<LowStockEvent> CreateEventAsync(int productId, int currentStock, int threshold)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        var lowStockEvent = new LowStockEvent
        {
            ProductId = productId,
            ProductName = product.Name,
            ProductSKU = product.SKU,
            CurrentStock = currentStock,
            Threshold = threshold,
            CreatedAt = DateTime.UtcNow,
            IsAcknowledged = false
        };

        _context.LowStockEvents.Add(lowStockEvent);
        await _context.SaveChangesAsync();

        return lowStockEvent;
    }

    public async Task AcknowledgeEventAsync(int eventId, string acknowledgedBy)
    {
        var lowStockEvent = await _context.LowStockEvents.FindAsync(eventId);
        if (lowStockEvent != null && !lowStockEvent.IsAcknowledged)
        {
            lowStockEvent.IsAcknowledged = true;
            lowStockEvent.AcknowledgedAt = DateTime.UtcNow;
            lowStockEvent.AcknowledgedBy = acknowledgedBy;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> HasRecentEventAsync(int productId, int hoursThreshold = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-hoursThreshold);
        return await _context.LowStockEvents
            .AnyAsync(e => e.ProductId == productId && e.CreatedAt >= cutoffTime);
    }

    public async Task<LowStockEvent> AddAsync(LowStockEvent entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.LowStockEvents.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(LowStockEvent entity)
    {
        _context.LowStockEvents.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var lowStockEvent = await _context.LowStockEvents.FindAsync(id);
        if (lowStockEvent != null)
        {
            _context.LowStockEvents.Remove(lowStockEvent);
            await _context.SaveChangesAsync();
        }
    }
}
