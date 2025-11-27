using EcomShopping.Application.DTOs;
using System.Net.Http.Json;

namespace EcomShopping.Web.Services;

/// <summary>
/// Service for communicating with the File Import API
/// </summary>
public class FileImportApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileImportApiService> _logger;

    public FileImportApiService(HttpClient httpClient, ILogger<FileImportApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ImportJobDto>?> GetAllJobsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ImportJobDto>>("api/fileimport");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching import jobs");
            return null;
        }
    }

    public async Task<List<ImportJobDto>?> GetRecentJobsAsync(int count = 10)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ImportJobDto>>($"api/fileimport/recent?count={count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent import jobs");
            return null;
        }
    }

    public async Task<ImportJobDto?> GetJobAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ImportJobDto>($"api/fileimport/{id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching import job {JobId}", id);
            return null;
        }
    }

    public async Task<ImportJobDto?> CreateJobAsync(CreateImportJobDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/fileimport", dto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ImportJobDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating import job");
            return null;
        }
    }

    public async Task<bool> DeleteJobAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/fileimport/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting import job {JobId}", id);
            return false;
        }
    }
}
