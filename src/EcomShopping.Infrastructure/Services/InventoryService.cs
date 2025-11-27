using EcomShopping.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EcomShopping.Infrastructure.Services;

/// <summary>
/// Service for managing inventory operations including stock reservations and low-stock monitoring
/// </summary>
public class InventoryService
{
    private readonly IStockReservationRepository _stockReservationRepository;
    private readonly ILowStockEventRepository _lowStockEventRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IStockReservationRepository stockReservationRepository,
        ILowStockEventRepository lowStockEventRepository,
        IProductRepository productRepository,
        ILogger<InventoryService> logger)
    {
        _stockReservationRepository = stockReservationRepository;
        _lowStockEventRepository = lowStockEventRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Reserve stock for products in a cart during checkout
    /// </summary>
    public async Task<List<int>> ReserveCartStockAsync(IEnumerable<(int ProductId, int Quantity)> cartItems, string sessionId, int expirationMinutes = 15)
    {
        var reservationIds = new List<int>();

        foreach (var (productId, quantity) in cartItems)
        {
            try
            {
                var reservation = await _stockReservationRepository.ReserveStockAsync(
                    productId, 
                    quantity, 
                    sessionId, 
                    expirationMinutes);
                
                reservationIds.Add(reservation.Id);
                _logger.LogInformation("Reserved {Quantity} units of product {ProductId} for session {SessionId}", 
                    quantity, productId, sessionId);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to reserve stock for product {ProductId}", productId);
                
                // Release any reservations made so far
                foreach (var reservationId in reservationIds)
                {
                    await _stockReservationRepository.ReleaseReservationAsync(reservationId);
                }
                
                throw;
            }
        }

        return reservationIds;
    }

    /// <summary>
    /// Release stock reservations for a session
    /// </summary>
    public async Task ReleaseSessionReservationsAsync(string sessionId)
    {
        await _stockReservationRepository.ReleaseSessionReservationsAsync(sessionId);
        _logger.LogInformation("Released all reservations for session {SessionId}", sessionId);
    }

    /// <summary>
    /// Release expired stock reservations
    /// </summary>
    public async Task ReleaseExpiredReservationsAsync()
    {
        await _stockReservationRepository.ReleaseExpiredReservationsAsync();
        _logger.LogInformation("Released expired stock reservations");
    }

    /// <summary>
    /// Check stock levels and create low-stock events if needed
    /// </summary>
    public async Task CheckLowStockLevelsAsync()
    {
        var products = await _productRepository.GetAllAsync();

        foreach (var product in products)
        {
            if (!product.IsActive)
                continue;

            var availableStock = await _stockReservationRepository.GetAvailableStockAsync(product.Id);
            
            if (availableStock <= product.LowStockThreshold)
            {
                // Check if we've already created an event recently (avoid spam)
                var hasRecentEvent = await _lowStockEventRepository.HasRecentEventAsync(product.Id, 24);
                
                if (!hasRecentEvent)
                {
                    await _lowStockEventRepository.CreateEventAsync(
                        product.Id,
                        availableStock,
                        product.LowStockThreshold);
                    
                    _logger.LogWarning("Low stock alert created for product {ProductName} (SKU: {SKU}). Available: {AvailableStock}, Threshold: {Threshold}",
                        product.Name, product.SKU, availableStock, product.LowStockThreshold);
                }
            }
        }
    }

    /// <summary>
    /// Get available stock for a product (actual stock minus reservations)
    /// </summary>
    public async Task<int> GetAvailableStockAsync(int productId)
    {
        return await _stockReservationRepository.GetAvailableStockAsync(productId);
    }
}
