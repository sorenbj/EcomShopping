using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Categories API
/// </summary>
public class CategoriesApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoriesApiService> _logger;

    public CategoriesApiService(HttpClient httpClient, ILogger<CategoriesApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<Category>?> GetCategoriesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<Category>>("api/categories");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching categories");
            return null;
        }
    }

    public async Task<Category?> GetCategoryAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<Category>($"api/categories/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching category {CategoryId}", id);
            return null;
        }
    }

    public async Task<Category?> CreateCategoryAsync(Category category)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/categories", category);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Category>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return null;
        }
    }

    public async Task<bool> UpdateCategoryAsync(int id, Category category)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/categories/{id}", category);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return false;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/categories/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return false;
        }
    }
}
