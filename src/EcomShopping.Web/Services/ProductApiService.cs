using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Product Catalog API
/// </summary>
public class ProductApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(HttpClient httpClient, ILogger<ProductApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get a paged list of products
    /// </summary>
    public async Task<PagedProductResponse?> GetProductsAsync(
        int page = 1, 
        int pageSize = 12, 
        string? search = null, 
        int? categoryId = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (categoryId.HasValue)
                queryParams.Add($"categoryId={categoryId.Value}");

            var url = $"api/products?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetFromJsonAsync<PagedProductResponse>(url);
            
            // Note: Client-side sorting is done here as a temporary solution
            // TODO: Implement server-side sorting in the API for better performance
            if (response?.Items != null && !string.IsNullOrWhiteSpace(sortBy))
            {
                response.Items = sortBy.ToLower() switch
                {
                    "name" => sortDescending 
                        ? response.Items.OrderByDescending(p => p.Name).ToList()
                        : response.Items.OrderBy(p => p.Name).ToList(),
                    "price" => sortDescending
                        ? response.Items.OrderByDescending(p => p.Price).ToList()
                        : response.Items.OrderBy(p => p.Price).ToList(),
                    _ => response.Items
                };
            }
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products");
            return null;
        }
    }

    /// <summary>
    /// Get a product by ID
    /// </summary>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", id);
            return null;
        }
    }

    /// <summary>
    /// Get a product by slug
    /// </summary>
    public async Task<ProductDto?> GetProductBySlugAsync(string slug)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/slug/{slug}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product by slug {Slug}", slug);
            return null;
        }
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    public async Task<List<CategoryDto>?> GetCategoriesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/categories");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return null;
        }
    }
}

/// <summary>
/// Paged response for products
/// </summary>
public class PagedProductResponse
{
    public List<ProductDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
