using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
    Task<StockMovement> AddMovementAsync(int productId, int quantity, StockMovementType type, string? reference = null, string? notes = null, string? createdBy = null);
}
