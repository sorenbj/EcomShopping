using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the Articles API
/// </summary>
public class ArticleApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArticleApiService> _logger;

    public ArticleApiService(HttpClient httpClient, ILogger<ArticleApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Get a paged list of published articles
    /// </summary>
    public async Task<PagedArticleResponse?> GetArticlesAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var url = $"api/articles?page={page}&pageSize={pageSize}";
            return await _httpClient.GetFromJsonAsync<PagedArticleResponse>(url);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error retrieving articles");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving articles");
            return null;
        }
    }

    /// <summary>
    /// Get a specific article by ID
    /// </summary>
    public async Task<ArticleDto?> GetArticleByIdAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ArticleDto>($"api/articles/{id}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error retrieving article {ArticleId}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article {ArticleId}", id);
            return null;
        }
    }

    /// <summary>
    /// Get a specific article by slug
    /// </summary>
    public async Task<ArticleDto?> GetArticleBySlugAsync(string slug)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ArticleDto>($"api/articles/by-slug/{slug}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error retrieving article by slug {Slug}", slug);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving article by slug {Slug}", slug);
            return null;
        }
    }

    /// <summary>
    /// Create a new article
    /// </summary>
    public async Task<ArticleDto?> CreateArticleAsync(CreateArticleDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/articles", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArticleDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error creating article");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating article");
            return null;
        }
    }

    /// <summary>
    /// Update an existing article
    /// </summary>
    public async Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/articles/{id}", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArticleDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error updating article {ArticleId}", id);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating article {ArticleId}", id);
            return null;
        }
    }

    /// <summary>
    /// Delete an article
    /// </summary>
    public async Task<bool> DeleteArticleAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/articles/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error deleting article {ArticleId}", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting article {ArticleId}", id);
            return false;
        }
    }
}

/// <summary>
/// Response model for paged article results
/// </summary>
public class PagedArticleResponse
{
    public List<ArticleDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
