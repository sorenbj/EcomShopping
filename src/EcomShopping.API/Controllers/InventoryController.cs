using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Application.DTOs;
using EcomShopping.Infrastructure.Services;
using EcomShopping.Integration.Abstractions;
using EcomShopping.Integration.Core;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for advanced inventory management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IStockReservationRepository _stockReservationRepository;
    private readonly ILowStockEventRepository _lowStockEventRepository;
    private readonly InventoryService _inventoryService;
    private readonly IntegrationEngine _integrationEngine;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository,
        IStockReservationRepository stockReservationRepository,
        ILowStockEventRepository lowStockEventRepository,
        InventoryService inventoryService,
        IntegrationEngine integrationEngine,
        ILogger<InventoryController> logger)
    {
        _productRepository = productRepository;
        _stockMovementRepository = stockMovementRepository;
        _stockReservationRepository = stockReservationRepository;
        _lowStockEventRepository = lowStockEventRepository;
        _inventoryService = inventoryService;
        _integrationEngine = integrationEngine;
        _logger = logger;
    }

    /// <summary>
    /// Get low-stock alerts
    /// </summary>
    /// <param name="unacknowledgedOnly">Return only unacknowledged events</param>
    /// <returns>List of low-stock events</returns>
    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(typeof(IEnumerable<LowStockEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<LowStockEventDto>>> GetLowStockAlerts([FromQuery] bool unacknowledgedOnly = true)
    {
        try
        {
            var events = unacknowledgedOnly 
                ? await _lowStockEventRepository.GetUnacknowledgedAsync()
                : await _lowStockEventRepository.GetAllAsync();

            var dtos = events.Select(e => MapToLowStockEventDto(e)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low-stock alerts");
            return StatusCode(500, "An error occurred while retrieving low-stock alerts");
        }
    }

    /// <summary>
    /// Acknowledge a low-stock event
    /// </summary>
    /// <param name="eventId">Event ID</param>
    /// <param name="acknowledgedBy">User acknowledging the event</param>
    /// <returns>Success status</returns>
    [HttpPost("low-stock-alerts/{eventId}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AcknowledgeLowStockAlert(int eventId, [FromBody] string acknowledgedBy)
    {
        try
        {
            var lowStockEvent = await _lowStockEventRepository.GetByIdAsync(eventId);
            if (lowStockEvent == null)
            {
                return NotFound($"Low-stock event with ID {eventId} not found");
            }

            await _lowStockEventRepository.AcknowledgeEventAsync(eventId, acknowledgedBy);
            return Ok(new { message = "Low-stock event acknowledged successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging low-stock event {EventId}", eventId);
            return StatusCode(500, "An error occurred while acknowledging the low-stock event");
        }
    }

    /// <summary>
    /// Check all products for low-stock levels and create events
    /// </summary>
    /// <returns>Number of events created</returns>
    [HttpPost("check-low-stock")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckLowStockLevels()
    {
        try
        {
            await _inventoryService.CheckLowStockLevelsAsync();
            var unacknowledgedEvents = await _lowStockEventRepository.GetUnacknowledgedAsync();
            
            return Ok(new 
            { 
                message = "Low-stock check completed",
                unacknowledgedAlerts = unacknowledgedEvents.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking low-stock levels");
            return StatusCode(500, "An error occurred while checking low-stock levels");
        }
    }

    /// <summary>
    /// Synchronize stock from ERP system
    /// </summary>
    /// <param name="syncData">Stock synchronization data from ERP</param>
    /// <returns>Synchronization result</returns>
    [HttpPost("erp-sync")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SyncStockFromErp([FromBody] ErpStockSyncDto syncData)
    {
        try
        {
            // Find product by SKU
            var product = await _productRepository.GetBySkuAsync(syncData.SKU);
            if (product == null)
            {
                return NotFound($"Product with SKU {syncData.SKU} not found");
            }

            // Calculate the difference
            var currentStock = product.StockQuantity;
            var difference = syncData.Quantity - currentStock;

            if (difference == 0)
            {
                return Ok(new 
                { 
                    message = "Stock already synchronized",
                    sku = syncData.SKU,
                    currentStock = currentStock
                });
            }

            // Create stock movement for the adjustment
            var movementType = difference > 0 ? StockMovementType.Purchase : StockMovementType.Adjustment;
            var movement = await _stockMovementRepository.AddMovementAsync(
                product.Id,
                difference,
                movementType,
                syncData.Reference ?? "ERP Sync",
                syncData.Notes ?? $"Stock synchronized from ERP. Previous: {currentStock}, New: {syncData.Quantity}"
            );

            _logger.LogInformation("Stock synchronized from ERP for product {SKU}. Previous: {PreviousStock}, New: {NewStock}",
                syncData.SKU, currentStock, syncData.Quantity);

            return Ok(new 
            { 
                message = "Stock synchronized successfully",
                sku = syncData.SKU,
                previousStock = currentStock,
                newStock = syncData.Quantity,
                difference = difference,
                movementId = movement.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing stock from ERP for SKU {SKU}", syncData.SKU);
            return StatusCode(500, "An error occurred while synchronizing stock from ERP");
        }
    }

    /// <summary>
    /// Push stock update to ERP system
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>ERP sync result</returns>
    [HttpPost("push-to-erp/{productId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PushStockToErp(int productId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            // Use integration engine to update ERP
            var result = await _integrationEngine.ExecuteAsync(
                "erp_provider", 
                "updateinventory",
                new { sku = product.SKU, quantity = product.StockQuantity });

            if (result.IsSuccess)
            {
                _logger.LogInformation("Stock pushed to ERP for product {SKU}: {Quantity} units",
                    product.SKU, product.StockQuantity);
                
                return Ok(new 
                { 
                    message = result.Message,
                    sku = product.SKU,
                    quantity = product.StockQuantity
                });
            }
            else
            {
                _logger.LogWarning("Failed to push stock to ERP for product {SKU}: {Message}",
                    product.SKU, result.Message);
                
                return StatusCode(500, new { message = result.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pushing stock to ERP for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while pushing stock to ERP");
        }
    }

    /// <summary>
    /// Get available stock for a product (actual stock minus reservations)
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Available stock quantity</returns>
    [HttpGet("available-stock/{productId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAvailableStock(int productId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var availableStock = await _inventoryService.GetAvailableStockAsync(productId);
            
            return Ok(new 
            { 
                productId = productId,
                sku = product.SKU,
                name = product.Name,
                actualStock = product.StockQuantity,
                availableStock = availableStock,
                reserved = product.StockQuantity - availableStock
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available stock for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while getting available stock");
        }
    }

    /// <summary>
    /// Release expired stock reservations
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("release-expired-reservations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReleaseExpiredReservations()
    {
        try
        {
            await _inventoryService.ReleaseExpiredReservationsAsync();
            return Ok(new { message = "Expired reservations released successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing expired reservations");
            return StatusCode(500, "An error occurred while releasing expired reservations");
        }
    }

    private LowStockEventDto MapToLowStockEventDto(LowStockEvent lowStockEvent)
    {
        return new LowStockEventDto
        {
            Id = lowStockEvent.Id,
            ProductId = lowStockEvent.ProductId,
            ProductName = lowStockEvent.ProductName,
            ProductSKU = lowStockEvent.ProductSKU,
            CurrentStock = lowStockEvent.CurrentStock,
            Threshold = lowStockEvent.Threshold,
            CreatedAt = lowStockEvent.CreatedAt,
            IsAcknowledged = lowStockEvent.IsAcknowledged,
            AcknowledgedAt = lowStockEvent.AcknowledgedAt,
            AcknowledgedBy = lowStockEvent.AcknowledgedBy
        };
    }
}
