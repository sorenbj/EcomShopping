using EcomShopping.Application.DTOs;
using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcomShopping.API.Controllers;

/// <summary>
/// API endpoints for file import management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileImportController : ControllerBase
{
    private readonly IImportJobRepository _importJobRepository;
    private readonly ILogger<FileImportController> _logger;

    public FileImportController(
        IImportJobRepository importJobRepository,
        ILogger<FileImportController> logger)
    {
        _importJobRepository = importJobRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all import jobs
    /// </summary>
    /// <returns>List of all import jobs</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ImportJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ImportJobDto>>> GetAllJobs()
    {
        try
        {
            var jobs = await _importJobRepository.GetAllAsync();
            var dtos = jobs.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving import jobs");
            return StatusCode(500, "An error occurred while retrieving import jobs");
        }
    }

    /// <summary>
    /// Get a specific import job by ID
    /// </summary>
    /// <param name="id">Import job ID</param>
    /// <returns>Import job details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImportJobDto>> GetJob(int id)
    {
        try
        {
            var job = await _importJobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(job));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving import job {JobId}", id);
            return StatusCode(500, "An error occurred while retrieving the import job");
        }
    }

    /// <summary>
    /// Get recent import jobs
    /// </summary>
    /// <param name="count">Number of recent jobs to retrieve (default: 10)</param>
    /// <returns>List of recent import jobs</returns>
    [HttpGet("recent")]
    [ProducesResponseType(typeof(IEnumerable<ImportJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ImportJobDto>>> GetRecentJobs([FromQuery] int count = 10)
    {
        try
        {
            var jobs = await _importJobRepository.GetRecentJobsAsync(count);
            var dtos = jobs.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent import jobs");
            return StatusCode(500, "An error occurred while retrieving recent import jobs");
        }
    }

    /// <summary>
    /// Get import jobs by status
    /// </summary>
    /// <param name="status">Import job status</param>
    /// <returns>List of import jobs with the specified status</returns>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<ImportJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ImportJobDto>>> GetJobsByStatus(ImportJobStatus status)
    {
        try
        {
            var jobs = await _importJobRepository.GetByStatusAsync(status);
            var dtos = jobs.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving import jobs by status {Status}", status);
            return StatusCode(500, "An error occurred while retrieving import jobs");
        }
    }

    /// <summary>
    /// Initiate a new import job
    /// </summary>
    /// <param name="dto">Import job creation data</param>
    /// <returns>Created import job</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ImportJobDto>> CreateJob(CreateImportJobDto dto)
    {
        try
        {
            var job = new ImportJob
            {
                FileName = dto.FileName,
                FileType = dto.FileType,
                Status = ImportJobStatus.Pending,
                CreatedBy = dto.CreatedBy,
                TotalRecords = 0,
                ProcessedRecords = 0,
                SuccessfulRecords = 0,
                FailedRecords = 0
            };

            var createdJob = await _importJobRepository.AddAsync(job);
            var jobDto = MapToDto(createdJob);
            return CreatedAtAction(nameof(GetJob), new { id = createdJob.Id }, jobDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating import job");
            return StatusCode(500, "An error occurred while creating the import job");
        }
    }

    /// <summary>
    /// Update an import job's status and progress
    /// </summary>
    /// <param name="id">Import job ID</param>
    /// <param name="dto">Update data</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateJob(int id, UpdateImportJobDto dto)
    {
        try
        {
            var job = await _importJobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            job.Status = dto.Status;

            if (dto.TotalRecords.HasValue)
                job.TotalRecords = dto.TotalRecords.Value;
            if (dto.ProcessedRecords.HasValue)
                job.ProcessedRecords = dto.ProcessedRecords.Value;
            if (dto.SuccessfulRecords.HasValue)
                job.SuccessfulRecords = dto.SuccessfulRecords.Value;
            if (dto.FailedRecords.HasValue)
                job.FailedRecords = dto.FailedRecords.Value;
            if (dto.ErrorLog != null)
                job.ErrorLog = dto.ErrorLog;

            // Update timestamps based on status
            if (dto.Status == ImportJobStatus.Processing && !job.StartedAt.HasValue)
                job.StartedAt = DateTime.UtcNow;
            else if ((dto.Status == ImportJobStatus.Completed || 
                      dto.Status == ImportJobStatus.Failed || 
                      dto.Status == ImportJobStatus.PartiallyCompleted) && 
                     !job.CompletedAt.HasValue)
                job.CompletedAt = DateTime.UtcNow;

            await _importJobRepository.UpdateAsync(job);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating import job {JobId}", id);
            return StatusCode(500, "An error occurred while updating the import job");
        }
    }

    /// <summary>
    /// Delete an import job (Admin)
    /// </summary>
    /// <param name="id">Import job ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteJob(int id)
    {
        try
        {
            var job = await _importJobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            await _importJobRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting import job {JobId}", id);
            return StatusCode(500, "An error occurred while deleting the import job");
        }
    }

    private ImportJobDto MapToDto(ImportJob job)
    {
        return new ImportJobDto
        {
            Id = job.Id,
            FileName = job.FileName,
            FileType = job.FileType,
            Status = job.Status,
            TotalRecords = job.TotalRecords,
            ProcessedRecords = job.ProcessedRecords,
            SuccessfulRecords = job.SuccessfulRecords,
            FailedRecords = job.FailedRecords,
            ErrorLog = job.ErrorLog,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            CreatedBy = job.CreatedBy
        };
    }
}
