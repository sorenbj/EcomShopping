using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface ICartRepository : IRepository<Cart>
{
    Task<Cart?> GetBySessionIdAsync(string sessionId);
    Task<Cart?> GetByUserIdAsync(string userId);
}
