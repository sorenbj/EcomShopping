using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.FileImport.Core;
using Microsoft.Extensions.Logging;

namespace EcomShopping.Infrastructure.Services;

/// <summary>
/// Service for orchestrating file import operations
/// </summary>
public class FileImportOrchestrationService
{
    private readonly FileImportService _fileImportService;
    private readonly IImportJobRepository _importJobRepository;
    private readonly ILogger<FileImportOrchestrationService> _logger;

    public FileImportOrchestrationService(
        FileImportService fileImportService,
        IImportJobRepository importJobRepository,
        ILogger<FileImportOrchestrationService> logger)
    {
        _fileImportService = fileImportService;
        _importJobRepository = importJobRepository;
        _logger = logger;
    }

    /// <summary>
    /// Saves uploaded file data and creates an import job
    /// </summary>
    public async Task<ImportJob> CreateImportJobAsync(
        Stream fileStream,
        string fileName,
        string fileType,
        string? createdBy = null)
    {
        var job = new ImportJob
        {
            FileName = fileName,
            FileType = fileType,
            Status = ImportJobStatus.Pending,
            CreatedBy = createdBy,
            TotalRecords = 0,
            ProcessedRecords = 0,
            SuccessfulRecords = 0,
            FailedRecords = 0
        };

        var createdJob = await _importJobRepository.AddAsync(job);
        _logger.LogInformation("Created import job {JobId} for file {FileName}", createdJob.Id, fileName);

        return createdJob;
    }

    /// <summary>
    /// Parses file and updates job with total record count
    /// </summary>
    public async Task<IEnumerable<Dictionary<string, object>>> ParseFileForJobAsync(
        int jobId,
        Stream fileStream,
        string fileExtension)
    {
        var job = await _importJobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            throw new InvalidOperationException($"Import job {jobId} not found");
        }

        try
        {
            var records = await _fileImportService.ParseFileAsync(fileStream, fileExtension);
            var recordsList = records.ToList();
            
            job.TotalRecords = recordsList.Count;
            await _importJobRepository.UpdateAsync(job);

            _logger.LogInformation("Parsed {RecordCount} records from file for job {JobId}", 
                recordsList.Count, jobId);

            return recordsList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing file for job {JobId}", jobId);
            job.Status = ImportJobStatus.Failed;
            job.ErrorLog = $"File parsing error: {ex.Message}";
            await _importJobRepository.UpdateAsync(job);
            throw;
        }
    }

    /// <summary>
    /// Executes import with the given configuration
    /// </summary>
    public async Task<ImportResult> ExecuteImportAsync(
        int jobId,
        IEnumerable<Dictionary<string, object>> records,
        ImportConfiguration configuration)
    {
        var job = await _importJobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            throw new InvalidOperationException($"Import job {jobId} not found");
        }

        try
        {
            // Update job status to processing
            job.Status = ImportJobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            await _importJobRepository.UpdateAsync(job);

            _logger.LogInformation("Starting import execution for job {JobId}", jobId);

            // Execute the import
            var result = await _fileImportService.ImportDataAsync(records, configuration);

            // Update job with results
            job.ProcessedRecords = result.TotalRecords;
            job.SuccessfulRecords = result.SuccessfulRecords;
            job.FailedRecords = result.FailedRecords;
            job.CompletedAt = DateTime.UtcNow;

            if (result.FailedRecords == 0)
            {
                job.Status = ImportJobStatus.Completed;
            }
            else if (result.SuccessfulRecords > 0)
            {
                job.Status = ImportJobStatus.PartiallyCompleted;
            }
            else
            {
                job.Status = ImportJobStatus.Failed;
            }

            if (result.Errors.Any())
            {
                job.ErrorLog = string.Join("\n", result.Errors);
            }

            await _importJobRepository.UpdateAsync(job);

            _logger.LogInformation(
                "Completed import job {JobId}: {Successful} successful, {Failed} failed out of {Total} total",
                jobId, result.SuccessfulRecords, result.FailedRecords, result.TotalRecords);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing import for job {JobId}", jobId);
            
            job.Status = ImportJobStatus.Failed;
            job.ErrorLog = $"Import execution error: {ex.Message}";
            job.CompletedAt = DateTime.UtcNow;
            await _importJobRepository.UpdateAsync(job);
            
            throw;
        }
    }

    /// <summary>
    /// Gets available fields from parsed data
    /// </summary>
    public List<string> GetAvailableFields(IEnumerable<Dictionary<string, object>> records)
    {
        return _fileImportService.GetAvailableFields(records);
    }

    /// <summary>
    /// Gets all available table importers
    /// </summary>
    public IEnumerable<ITableImporter> GetAvailableImporters()
    {
        return _fileImportService.GetAvailableImporters();
    }
}
