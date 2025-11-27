using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface ILowStockEventRepository : IRepository<LowStockEvent>
{
    /// <summary>
    /// Get all unacknowledged low-stock events
    /// </summary>
    Task<IEnumerable<LowStockEvent>> GetUnacknowledgedAsync();

    /// <summary>
    /// Get low-stock events for a specific product
    /// </summary>
    Task<IEnumerable<LowStockEvent>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Create a low-stock event
    /// </summary>
    Task<LowStockEvent> CreateEventAsync(int productId, int currentStock, int threshold);

    /// <summary>
    /// Acknowledge a low-stock event
    /// </summary>
    Task AcknowledgeEventAsync(int eventId, string acknowledgedBy);

    /// <summary>
    /// Check if a recent event exists for a product (within last 24 hours)
    /// </summary>
    Task<bool> HasRecentEventAsync(int productId, int hoursThreshold = 24);
}
