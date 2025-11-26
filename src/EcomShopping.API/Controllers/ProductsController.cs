using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductRepository productRepository, ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null, [FromQuery] int? categoryId = null)
    {
        try
        {
            var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, search, categoryId);
            return Ok(new
            {
                items,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "An error occurred while retrieving products");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            var createdProduct = await _productRepository.AddAsync(product);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        try
        {
            if (id != product.Id)
            {
                return BadRequest("Product ID mismatch");
            }

            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            await _productRepository.UpdateAsync(product);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            await _productRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product");
        }
    }
}
