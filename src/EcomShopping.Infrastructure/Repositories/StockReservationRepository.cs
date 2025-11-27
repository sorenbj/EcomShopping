using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class StockReservationRepository : IStockReservationRepository
{
    private readonly ApplicationDbContext _context;

    public StockReservationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockReservation?> GetByIdAsync(int id)
    {
        return await _context.StockReservations
            .Include(sr => sr.Product)
            .FirstOrDefaultAsync(sr => sr.Id == id);
    }

    public async Task<IEnumerable<StockReservation>> GetAllAsync()
    {
        return await _context.StockReservations
            .Include(sr => sr.Product)
            .OrderByDescending(sr => sr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockReservation>> GetBySessionIdAsync(string sessionId)
    {
        return await _context.StockReservations
            .Include(sr => sr.Product)
            .Where(sr => sr.SessionId == sessionId && !sr.IsReleased)
            .ToListAsync();
    }

    public async Task<StockReservation> ReserveStockAsync(int productId, int quantity, string sessionId, int expirationMinutes = 15)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        // Check available stock (actual stock minus reserved stock)
        var availableStock = await GetAvailableStockAsync(productId);
        if (availableStock < quantity)
        {
            throw new InvalidOperationException($"Insufficient stock available. Requested: {quantity}, Available: {availableStock}");
        }

        var reservation = new StockReservation
        {
            ProductId = productId,
            Quantity = quantity,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            IsReleased = false
        };

        _context.StockReservations.Add(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }

    public async Task ReleaseReservationAsync(int reservationId)
    {
        var reservation = await _context.StockReservations.FindAsync(reservationId);
        if (reservation != null && !reservation.IsReleased)
        {
            reservation.IsReleased = true;
            reservation.ReleasedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ReleaseSessionReservationsAsync(string sessionId)
    {
        var reservations = await _context.StockReservations
            .Where(sr => sr.SessionId == sessionId && !sr.IsReleased)
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            reservation.IsReleased = true;
            reservation.ReleasedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task ReleaseExpiredReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var expiredReservations = await _context.StockReservations
            .Where(sr => !sr.IsReleased && sr.ExpiresAt < now)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.IsReleased = true;
            reservation.ReleasedAt = now;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetAvailableStockAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return 0;
        }

        // Get total reserved quantity (only active reservations)
        var reservedQuantity = await _context.StockReservations
            .Where(sr => sr.ProductId == productId && !sr.IsReleased && sr.ExpiresAt > DateTime.UtcNow)
            .SumAsync(sr => sr.Quantity);

        return product.StockQuantity - reservedQuantity;
    }

    public async Task ConfirmReservationAsync(int reservationId, string orderNumber)
    {
        var reservation = await _context.StockReservations.FindAsync(reservationId);
        if (reservation != null && !reservation.IsReleased)
        {
            reservation.OrderNumber = orderNumber;
            reservation.IsReleased = true;
            reservation.ReleasedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<StockReservation> AddAsync(StockReservation entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.StockReservations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(StockReservation entity)
    {
        _context.StockReservations.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var reservation = await _context.StockReservations.FindAsync(id);
        if (reservation != null)
        {
            _context.StockReservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
    }
}
