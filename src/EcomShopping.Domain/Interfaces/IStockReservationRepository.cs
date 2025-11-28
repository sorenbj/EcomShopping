using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IStockReservationRepository : IRepository<StockReservation>
{
    /// <summary>
    /// Get reservations by session ID
    /// </summary>
    Task<IEnumerable<StockReservation>> GetBySessionIdAsync(string sessionId);

    /// <summary>
    /// Reserve stock for a product during checkout
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to reserve</param>
    /// <param name="sessionId">Session ID</param>
    /// <param name="expirationMinutes">Minutes until reservation expires (default 15)</param>
    /// <returns>Created reservation</returns>
    Task<StockReservation> ReserveStockAsync(int productId, int quantity, string sessionId, int expirationMinutes = 15);

    /// <summary>
    /// Release a stock reservation
    /// </summary>
    Task ReleaseReservationAsync(int reservationId);

    /// <summary>
    /// Release all reservations for a session
    /// </summary>
    Task ReleaseSessionReservationsAsync(string sessionId);

    /// <summary>
    /// Release all expired reservations
    /// </summary>
    Task ReleaseExpiredReservationsAsync();

    /// <summary>
    /// Get available stock (actual stock minus reserved stock)
    /// </summary>
    Task<int> GetAvailableStockAsync(int productId);

    /// <summary>
    /// Get available stock for multiple products in a single query
    /// </summary>
    /// <param name="productIds">Product IDs to check</param>
    /// <returns>Dictionary of ProductId to AvailableStock</returns>
    Task<Dictionary<int, int>> GetAvailableStockBatchAsync(IEnumerable<int> productIds);

    /// <summary>
    /// Confirm reservation when order is placed
    /// </summary>
    Task ConfirmReservationAsync(int reservationId, string orderNumber);
}
