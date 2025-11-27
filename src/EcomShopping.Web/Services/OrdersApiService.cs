using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Orders API
/// </summary>
public class OrdersApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrdersApiService> _logger;

    public OrdersApiService(HttpClient httpClient, ILogger<OrdersApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PagedOrderResponse?> GetOrdersAsync(
        string? userId = null,
        OrderStatus? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? orderNumber = null,
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(userId))
                queryParams.Add($"userId={Uri.EscapeDataString(userId)}");

            if (status.HasValue)
                queryParams.Add($"status={status.Value}");

            if (startDate.HasValue)
                queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");

            if (endDate.HasValue)
                queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");

            if (!string.IsNullOrWhiteSpace(orderNumber))
                queryParams.Add($"orderNumber={Uri.EscapeDataString(orderNumber)}");

            var url = $"api/orders?{string.Join("&", queryParams)}";
            return await _httpClient.GetFromJsonAsync<PagedOrderResponse>(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return null;
        }
    }

    public async Task<OrderDto?> GetOrderAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OrderDto>($"api/orders/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", id);
            return null;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        try
        {
            var request = new { Status = status };
            var response = await _httpClient.PutAsJsonAsync($"api/orders/{id}/status", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for {OrderId}", id);
            return false;
        }
    }
}

/// <summary>
/// Paged response for orders
/// </summary>
public class PagedOrderResponse
{
    public List<OrderDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
