using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Inventory API
/// </summary>
public class InventoryApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryApiService> _logger;

    public InventoryApiService(HttpClient httpClient, ILogger<InventoryApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<LowStockEventDto>?> GetLowStockAlertsAsync(bool unacknowledgedOnly = true)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            return await _httpClient.GetFromJsonAsync<List<LowStockEventDto>>(
                $"api/inventory/low-stock-alerts?unacknowledgedOnly={unacknowledgedOnly}", cts.Token);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout fetching low-stock alerts after 10 seconds");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching low-stock alerts");
            return null;
        }
    }

    public async Task<bool> AcknowledgeLowStockAlertAsync(int eventId, string acknowledgedBy)
    {
        try
        {
            var dto = new AcknowledgeLowStockDto { AcknowledgedBy = acknowledgedBy };
            var response = await _httpClient.PostAsJsonAsync($"api/inventory/low-stock-alerts/{eventId}/acknowledge", dto);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging low-stock alert {EventId}", eventId);
            return false;
        }
    }

    public async Task<bool> CheckLowStockLevelsAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("api/inventory/check-low-stock", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking low-stock levels");
            return false;
        }
    }

    public async Task<AvailableStockDto?> GetAvailableStockAsync(int productId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<AvailableStockDto>($"api/inventory/available-stock/{productId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available stock for product {ProductId}", productId);
            return null;
        }
    }

    public async Task<bool> SyncStockFromErpAsync(ErpStockSyncDto syncData)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/inventory/erp-sync", syncData);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing stock from ERP");
            return false;
        }
    }
}

/// <summary>
/// Available stock DTO
/// </summary>
public class AvailableStockDto
{
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ActualStock { get; set; }
    public int AvailableStock { get; set; }
    public int Reserved { get; set; }
}
