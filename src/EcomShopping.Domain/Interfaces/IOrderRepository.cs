using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
}
