using EcomShopping.Domain.Entities;

namespace EcomShopping.Application.DTOs;

public class ImportJobDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public ImportJobStatus Status { get; set; }
    public int TotalRecords { get; set; }
    public int ProcessedRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public string? ErrorLog { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? CreatedBy { get; set; }
    public decimal ProgressPercentage => TotalRecords > 0 ? (decimal)ProcessedRecords / TotalRecords * 100 : 0;
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue ? CompletedAt.Value - StartedAt.Value : null;
}

public class CreateImportJobDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
}

public class UpdateImportJobDto
{
    public ImportJobStatus Status { get; set; }
    public int? TotalRecords { get; set; }
    public int? ProcessedRecords { get; set; }
    public int? SuccessfulRecords { get; set; }
    public int? FailedRecords { get; set; }
    public string? ErrorLog { get; set; }
}
