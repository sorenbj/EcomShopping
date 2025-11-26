using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for product catalog management
/// </summary>
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

    /// <summary>
    /// Get a paged list of products with optional search and filtering
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
    /// <param name="search">Optional search term to filter by name or description</param>
    /// <param name="categoryId">Optional category ID to filter by</param>
    /// <returns>Paged list of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<object>> GetProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string? search = null, 
        [FromQuery] int? categoryId = null)
    {
        try
        {
            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            var (items, totalCount) = await _productRepository.GetPagedAsync(page, pageSize, search, categoryId);
            
            var productDtos = items.Select(p => MapToDto(p)).ToList();

            return Ok(new
            {
                items = productDtos,
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

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Get a specific product by slug
    /// </summary>
    /// <param name="slug">Product slug (URL-friendly name)</param>
    /// <returns>Product details</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
    {
        try
        {
            var product = await _productRepository.GetBySlugAsync(slug);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by slug {Slug}", slug);
            return StatusCode(500, "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Create a new product (Admin)
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto dto)
    {
        try
        {
            var product = MapFromCreateDto(dto);
            var createdProduct = await _productRepository.AddAsync(product);
            var productDto = MapToDto(createdProduct);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product");
        }
    }

    /// <summary>
    /// Update an existing product (Admin)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="dto">Product update data</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            UpdateProductFromDto(existingProduct, dto);
            await _productRepository.UpdateAsync(existingProduct);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product");
        }
    }

    /// <summary>
    /// Delete a product (Admin)
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            SKU = product.SKU,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            Images = product.Images,
            Metadata = product.Metadata,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private Product MapFromCreateDto(CreateProductDto dto)
    {
        var slug = dto.Slug ?? GenerateSlug(dto.Name);
        return new Product
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            Price = dto.Price,
            SKU = dto.SKU,
            CategoryId = dto.CategoryId,
            StockQuantity = dto.StockQuantity,
            IsActive = dto.IsActive,
            Images = dto.Images,
            Metadata = dto.Metadata
        };
    }

    private void UpdateProductFromDto(Product product, UpdateProductDto dto)
    {
        product.Name = dto.Name;
        product.Slug = dto.Slug ?? product.Slug;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.SKU = dto.SKU;
        product.CategoryId = dto.CategoryId;
        product.StockQuantity = dto.StockQuantity;
        product.IsActive = dto.IsActive;
        product.Images = dto.Images;
        product.Metadata = dto.Metadata;
    }

    private string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        // Convert to lowercase and remove accents
        var slug = name.ToLowerInvariant();
        
        // Replace spaces and special characters with hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        return slug.Trim('-');
    }
}
