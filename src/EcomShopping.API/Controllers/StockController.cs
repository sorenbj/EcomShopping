using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for inventory/stock management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<StockController> _logger;

    public StockController(
        IStockMovementRepository stockMovementRepository,
        IProductRepository productRepository,
        ILogger<StockController> logger)
    {
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get stock movements for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of stock movements</returns>
    [HttpGet("product/{productId}")]
    [ProducesResponseType(typeof(IEnumerable<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetProductStockMovements(int productId)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound($"Product with ID {productId} not found");
            }

            var movements = await _stockMovementRepository.GetByProductIdAsync(productId);
            var dtos = movements.Select(m => MapToDto(m)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock movements for product {ProductId}", productId);
            return StatusCode(500, "An error occurred while retrieving stock movements");
        }
    }

    /// <summary>
    /// Adjust stock for a product
    /// </summary>
    /// <param name="adjustment">Stock adjustment details</param>
    /// <returns>Created stock movement</returns>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StockMovementDto>> AdjustStock(StockAdjustmentDto adjustment)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(adjustment.ProductId);
            if (product == null)
            {
                return NotFound($"Product with ID {adjustment.ProductId} not found");
            }

            if (!Enum.TryParse<StockMovementType>(adjustment.Type, true, out var movementType))
            {
                return BadRequest($"Invalid stock movement type: {adjustment.Type}. Valid types: Purchase, Sale, Adjustment, Return, Damage");
            }

            var movement = await _stockMovementRepository.AddMovementAsync(
                adjustment.ProductId,
                adjustment.Quantity,
                movementType,
                adjustment.Reference,
                adjustment.Notes
            );

            var dto = MapToDto(movement);
            return CreatedAtAction(nameof(GetProductStockMovements), new { productId = movement.ProductId }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adjusting stock for product {ProductId}", adjustment.ProductId);
            return StatusCode(500, "An error occurred while adjusting stock");
        }
    }

    /// <summary>
    /// Get all stock movements (Admin)
    /// </summary>
    /// <returns>List of all stock movements</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetAllStockMovements()
    {
        try
        {
            var movements = await _stockMovementRepository.GetAllAsync();
            var dtos = movements.Select(m => MapToDto(m)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all stock movements");
            return StatusCode(500, "An error occurred while retrieving stock movements");
        }
    }

    /// <summary>
    /// Get low stock alerts for products below threshold (Admin)
    /// </summary>
    /// <param name="threshold">Stock quantity threshold (default: 10)</param>
    /// <returns>List of products with low stock</returns>
    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetLowStockAlerts([FromQuery] int threshold = 10)
    {
        try
        {
            var lowStockProducts = await _productRepository.GetLowStockProductsAsync(threshold);
            var alerts = lowStockProducts.Select(p => new
            {
                productId = p.Id,
                name = p.Name,
                sku = p.SKU,
                currentStock = p.StockQuantity,
                threshold,
                category = p.Category?.Name,
                isActive = p.IsActive
            }).ToList();

            return Ok(new
            {
                threshold,
                totalAlerts = alerts.Count,
                alerts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving low stock alerts");
            return StatusCode(500, "An error occurred while retrieving low stock alerts");
        }
    }

    private StockMovementDto MapToDto(StockMovement movement)
    {
        return new StockMovementDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = movement.Product?.Name,
            ProductSKU = movement.Product?.SKU,
            Quantity = movement.Quantity,
            Type = movement.Type.ToString(),
            Reference = movement.Reference,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt,
            CreatedBy = movement.CreatedBy
        };
    }
}
