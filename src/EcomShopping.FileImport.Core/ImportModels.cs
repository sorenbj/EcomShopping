namespace EcomShopping.FileImport.Core;

/// <summary>
/// Configuration for mapping source file fields to destination table columns
/// </summary>
public class FieldMapping
{
    public string SourceField { get; set; } = string.Empty;
    public string DestinationField { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public Func<object, object>? Transform { get; set; }
}

/// <summary>
/// Configuration for importing data to a specific table
/// </summary>
public class ImportConfiguration
{
    public string TargetTable { get; set; } = string.Empty;
    public List<FieldMapping> FieldMappings { get; set; } = new();
    public bool ValidateBeforeImport { get; set; } = true;
    public bool ContinueOnError { get; set; } = true;
}

/// <summary>
/// Result of a single record import
/// </summary>
public class ImportRecordResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? SourceData { get; set; }
    public object? ImportedEntity { get; set; }
}

/// <summary>
/// Overall result of an import operation
/// </summary>
public class ImportResult
{
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<ImportRecordResult> RecordResults { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public TimeSpan Duration { get; set; }
}
