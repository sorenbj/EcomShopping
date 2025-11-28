using EcomShopping.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
    private readonly int _defaultReservationExpirationMinutes;

    public InventoryService(
        IStockReservationRepository stockReservationRepository,
        ILowStockEventRepository lowStockEventRepository,
        IProductRepository productRepository,
        ILogger<InventoryService> logger,
        IConfiguration configuration)
    {
        _stockReservationRepository = stockReservationRepository;
        _lowStockEventRepository = lowStockEventRepository;
        _productRepository = productRepository;
        _logger = logger;
        _defaultReservationExpirationMinutes = int.TryParse(
            configuration["Inventory:ReservationExpirationMinutes"], 
            out var minutes) ? minutes : 15;
    }

    /// <summary>
    /// Reserve stock for products in a cart during checkout
    /// </summary>
    public async Task<List<int>> ReserveCartStockAsync(IEnumerable<(int ProductId, int Quantity)> cartItems, string sessionId, int? expirationMinutes = null)
    {
        var reservationIds = new List<int>();
        var expiration = expirationMinutes ?? _defaultReservationExpirationMinutes;

        foreach (var (productId, quantity) in cartItems)
        {
            try
            {
                var reservation = await _stockReservationRepository.ReserveStockAsync(
                    productId,
                    quantity, 
                    sessionId, 
                    expiration);
                
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
        // Get only active products from the database (more efficient than loading all and filtering in memory)
        var activeProducts = await _productRepository.GetActiveProductsAsync();
        var productList = activeProducts.ToList();

        if (!productList.Any())
            return;

        // Get available stock for all active products in a single batch query
        var productIds = productList.Select(p => p.Id).ToList();
        var availableStockMap = await _stockReservationRepository.GetAvailableStockBatchAsync(productIds);

        foreach (var product in productList)
        {
            var availableStock = availableStockMap.TryGetValue(product.Id, out var stock) ? stock : product.StockQuantity;
            await CheckAndCreateLowStockEventAsync(product.Id, availableStock, product.LowStockThreshold);
        }
    }

    /// <summary>
    /// Check if stock is low and create event if needed (shared logic)
    /// </summary>
    public async Task CheckAndCreateLowStockEventAsync(int productId, int availableStock, int threshold)
    {
        if (availableStock <= threshold)
        {
            // Check if we've already created an event recently (avoid spam)
            var hasRecentEvent = await _lowStockEventRepository.HasRecentEventAsync(productId, 24);
            
            if (!hasRecentEvent)
            {
                var product = await _productRepository.GetByIdAsync(productId);
                if (product != null)
                {
                    await _lowStockEventRepository.CreateEventAsync(
                        productId,
                        availableStock,
                        threshold);
                    
                    _logger.LogWarning("Low stock alert created for product {ProductName} (SKU: {SKU}). Available: {AvailableStock}, Threshold: {Threshold}",
                        product.Name, product.SKU, availableStock, threshold);
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
