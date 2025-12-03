using EcomShopping.Domain.Entities;
using System.Threading;

namespace EcomShopping.Domain.Interfaces;

public interface ILowStockEventRepository : IRepository<LowStockEvent>
{
    /// <summary>
    /// Get all unacknowledged low-stock events
    /// </summary>
    Task<IEnumerable<LowStockEvent>> GetUnacknowledgedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low-stock events for a specific product
    /// </summary>
    Task<IEnumerable<LowStockEvent>> GetByProductIdAsync(int productId);

    /// <summary>
    /// Create a low-stock event
    /// </summary>
    Task<LowStockEvent> CreateEventAsync(int productId, int currentStock, int threshold);

    /// <summary>
    /// Create a low-stock event with product information (avoids extra database query)
    /// </summary>
    Task<LowStockEvent> CreateEventAsync(int productId, string productName, string productSku, int currentStock, int threshold);

    /// <summary>
    /// Acknowledge a low-stock event
    /// </summary>
    Task AcknowledgeEventAsync(int eventId, string acknowledgedBy);

    /// <summary>
    /// Check if a recent event exists for a product (within last 24 hours)
    /// </summary>
    Task<bool> HasRecentEventAsync(int productId, int hoursThreshold = 24);

    /// <summary>
    /// Get product IDs that have recent events (batch operation to avoid N+1 queries)
    /// </summary>
    /// <param name="productIds">Product IDs to check</param>
    /// <param name="hoursThreshold">Hours threshold for recent events</param>
    /// <returns>Set of product IDs that have recent events</returns>
    Task<HashSet<int>> GetProductIdsWithRecentEventsAsync(IEnumerable<int> productIds, int hoursThreshold = 24);
}
