using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Orders/Checkout API
/// </summary>
public class CheckoutApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CheckoutApiService> _logger;

    public CheckoutApiService(HttpClient httpClient, ILogger<CheckoutApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Complete checkout and create order
    /// </summary>
    public async Task<(bool Success, OrderDto? Order, string? ErrorMessage)> CheckoutAsync(CheckoutRequest request)
    {
        try
        {
            // Note: Address IDs would need to be created in a real scenario
            // For now, we're using null values which will trigger the backend to handle address creation
            // In a production app, we'd have an Address API to create these first
            var checkoutPayload = new
            {
                sessionId = request.SessionId,
                userId = request.UserId,
                shippingAddressId = (int?)null,
                billingAddressId = (int?)null
            };

            var orderResponse = await _httpClient.PostAsJsonAsync("api/orders/checkout", checkoutPayload);
            
            if (!orderResponse.IsSuccessStatusCode)
            {
                var errorContent = await orderResponse.Content.ReadAsStringAsync();
                
                // Handle specific error cases
                if (orderResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (errorContent.Contains("Cart is empty"))
                    {
                        return (false, null, "Your cart is empty. Please add items before checking out.");
                    }
                    return (false, null, "Unable to process checkout. Please check your information.");
                }
                
                return (false, null, "Failed to create order. Please try again later.");
            }

            var checkoutResponse = await orderResponse.Content.ReadFromJsonAsync<CheckoutResponse>();
            
            if (checkoutResponse == null || checkoutResponse.Order == null)
            {
                _logger.LogError("Failed to deserialize checkout response");
                return (false, null, "Failed to process order response. Please try again.");
            }
            
            return (true, checkoutResponse.Order, null);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during checkout");
            return (false, null, "Network error. Please check your connection and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during checkout");
            return (false, null, "An unexpected error occurred. Please try again.");
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{orderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", orderId);
            return null;
        }
    }

    /// <summary>
    /// Get orders for user
    /// </summary>
    public async Task<List<OrderDto>?> GetOrdersAsync(string? userId = null)
    {
        try
        {
            var url = string.IsNullOrWhiteSpace(userId) 
                ? "api/orders" 
                : $"api/orders?userId={Uri.EscapeDataString(userId)}";
            
            return await _httpClient.GetFromJsonAsync<List<OrderDto>>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return null;
        }
    }
}
