namespace EcomShopping.Application.DTOs;

/// <summary>
/// DTO for uploading and initiating a file import
/// </summary>
public class FileUploadDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string TargetTable { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
}

/// <summary>
/// DTO for configuring field mappings
/// </summary>
public class FieldMappingDto
{
    public string SourceField { get; set; } = string.Empty;
    public string DestinationField { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// DTO for import configuration
/// </summary>
public class ImportConfigurationDto
{
    public string TargetTable { get; set; } = string.Empty;
    public List<FieldMappingDto> FieldMappings { get; set; } = new();
    public bool ValidateBeforeImport { get; set; } = true;
    public bool ContinueOnError { get; set; } = true;
}

/// <summary>
/// DTO for import execution request
/// </summary>
public class ExecuteImportDto
{
    public int JobId { get; set; }
    public ImportConfigurationDto Configuration { get; set; } = new();
}

/// <summary>
/// DTO for import result
/// </summary>
public class ImportResultDto
{
    public int TotalRecords { get; set; }
    public int SuccessfulRecords { get; set; }
    public int FailedRecords { get; set; }
    public List<string> Errors { get; set; } = new();
    public double DurationSeconds { get; set; }
}

/// <summary>
/// DTO for available table info
/// </summary>
public class ImportTableInfoDto
{
    public string TableName { get; set; } = string.Empty;
    public List<FieldMappingDto> DefaultFieldMappings { get; set; } = new();
}
