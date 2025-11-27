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

    public async Task<List<ImportTableInfoDto>?> GetAvailableTablesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<List<ImportTableInfoDto>>("api/fileimport/tables");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available import tables");
            return null;
        }
    }

    public async Task<FileUploadResultDto?> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string targetTable,
        string? createdBy = null)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(targetTable), "targetTable");
            
            if (!string.IsNullOrEmpty(createdBy))
            {
                content.Add(new StringContent(createdBy), "createdBy");
            }

            var response = await _httpClient.PostAsync("api/fileimport/upload", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<FileUploadResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", fileName);
            return null;
        }
    }

    public async Task<ImportResultDto?> UploadAndImportFileAsync(
        Stream fileStream,
        string fileName,
        string targetTable,
        string? createdBy = null)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);
            content.Add(new StringContent(targetTable), "targetTable");
            
            if (!string.IsNullOrEmpty(createdBy))
            {
                content.Add(new StringContent(createdBy), "createdBy");
            }

            var response = await _httpClient.PostAsync("api/fileimport/upload-and-import", content);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<ImportResultDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading and importing file {FileName}", fileName);
            return null;
        }
    }
}

public class FileUploadResultDto
{
    public int JobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public List<string> AvailableFields { get; set; } = new();
    public List<Dictionary<string, object>> PreviewRecords { get; set; } = new();
}

public class ImportResultDto
{
    public int JobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public double DurationSeconds { get; set; }
}
