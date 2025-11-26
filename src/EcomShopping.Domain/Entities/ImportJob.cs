namespace EcomShopping.Domain.Entities;

public class ImportJob
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
}

public enum ImportJobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    PartiallyCompleted = 4
}
