using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Admin API
/// </summary>
public class AdminApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminApiService> _logger;

    public AdminApiService(HttpClient httpClient, ILogger<AdminApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Dashboard Stats
    public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/admin/dashboard/stats");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats");
            return null;
        }
    }

    // User Management
    public async Task<List<UserDto>?> GetUsersAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<UserDto>>("api/admin/users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return null;
        }
    }

    public async Task<UserDto?> GetUserAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/admin/users/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", id);
            return null;
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserDto user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/admin/users", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return null;
        }
    }

    public async Task<bool> UpdateUserAsync(int id, UpdateUserDto user)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/admin/users/{id}", user);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/admin/users/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return false;
        }
    }

    // Role Management
    public async Task<List<RoleDto>?> GetRolesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<RoleDto>>("api/admin/roles");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching roles");
            return null;
        }
    }

    public async Task<RoleDto?> GetRoleAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RoleDto>($"api/admin/roles/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role {RoleId}", id);
            return null;
        }
    }
}

/// <summary>
/// Dashboard statistics DTO
/// </summary>
public class DashboardStatsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int RecentImportJobs { get; set; }
    public int PendingImports { get; set; }
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
}
