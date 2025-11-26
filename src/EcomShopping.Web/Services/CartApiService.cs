using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Cart API
/// </summary>
public class CartApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CartApiService> _logger;

    public CartApiService(HttpClient httpClient, ILogger<CartApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get cart by session ID or user ID
    /// </summary>
    public async Task<CartDto?> GetCartAsync(string sessionId, string? userId = null)
    {
        try
        {
            var queryParams = new List<string> { $"sessionId={Uri.EscapeDataString(sessionId)}" };
            
            if (!string.IsNullOrWhiteSpace(userId))
            {
                queryParams.Add($"userId={Uri.EscapeDataString(userId)}");
            }

            var url = $"api/cart?{string.Join("&", queryParams)}";
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var cart = await response.Content.ReadFromJsonAsync<CartDto>();
            return cart;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cart for sessionId: {SessionId}", sessionId);
            return null;
        }
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    public async Task<CartDto?> AddToCartAsync(AddToCartRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/cart/items", new
            {
                sessionId = request.SessionId,
                userId = request.UserId,
                productId = request.ProductId,
                quantity = request.Quantity
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CartDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            throw;
        }
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    public async Task<bool> UpdateCartItemAsync(int cartItemId, UpdateCartItemRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/cart/items/{cartItemId}", new
            {
                sessionId = request.SessionId,
                userId = request.UserId,
                quantity = request.Quantity
            });

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item {CartItemId}", cartItemId);
            return false;
        }
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    public async Task<bool> RemoveFromCartAsync(int cartItemId, string sessionId, string? userId = null)
    {
        try
        {
            var queryParams = new List<string> { $"sessionId={Uri.EscapeDataString(sessionId)}" };
            
            if (!string.IsNullOrWhiteSpace(userId))
            {
                queryParams.Add($"userId={Uri.EscapeDataString(userId)}");
            }

            var url = $"api/cart/items/{cartItemId}?{string.Join("&", queryParams)}";
            var response = await _httpClient.DeleteAsync(url);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item {CartItemId}", cartItemId);
            return false;
        }
    }
}
