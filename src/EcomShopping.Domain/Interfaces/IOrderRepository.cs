using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<(IEnumerable<Order> Items, int TotalCount)> GetFilteredOrdersAsync(
        string? userId = null,
        OrderStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? orderNumber = null,
        int page = 1,
        int pageSize = 10);
}
