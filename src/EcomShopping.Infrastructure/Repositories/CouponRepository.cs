using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class CouponRepository : ICouponRepository
{
    private readonly ApplicationDbContext _context;

    public CouponRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByIdAsync(int id)
    {
        return await _context.Coupons.FindAsync(id);
    }

    public async Task<IEnumerable<Coupon>> GetAllAsync()
    {
        return await _context.Coupons.ToListAsync();
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);
    }

    public async Task<IEnumerable<Coupon>> GetActiveAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Coupons
            .Where(c => c.IsActive 
                && (!c.ValidFrom.HasValue || c.ValidFrom.Value <= now)
                && (!c.ValidUntil.HasValue || c.ValidUntil.Value >= now))
            .ToListAsync();
    }

    public async Task<Coupon> AddAsync(Coupon entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.Coupons.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Coupon entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Coupons.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var coupon = await _context.Coupons.FindAsync(id);
        if (coupon != null)
        {
            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
        }
    }
}
