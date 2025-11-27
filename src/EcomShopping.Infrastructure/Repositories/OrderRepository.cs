using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public OrderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<Order> AddAsync(Order entity)
    {
        _context.Orders.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Order entity)
    {
        _context.Orders.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetFilteredOrdersAsync(
        string? userId = null,
        OrderStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? orderNumber = null,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.BillingAddress)
            .AsQueryable();

        // Apply filters at database level
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= endDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(orderNumber))
        {
            query = query.Where(o => o.OrderNumber.Contains(orderNumber));
        }

        // Order by date descending
        query = query.OrderByDescending(o => o.OrderDate);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply pagination at database level
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
