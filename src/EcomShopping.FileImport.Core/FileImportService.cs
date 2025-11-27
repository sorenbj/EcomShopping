using System.Diagnostics;

namespace EcomShopping.FileImport.Core;

/// <summary>
/// Service for managing file imports with parsing, mapping, validation, and importing
/// </summary>
public class FileImportService
{
    private readonly IEnumerable<IFileParser> _parsers;
    private readonly IEnumerable<ITableImporter> _importers;

    public FileImportService(
        IEnumerable<IFileParser> parsers,
        IEnumerable<ITableImporter> importers)
    {
        _parsers = parsers;
        _importers = importers;
    }

    /// <summary>
    /// Gets the appropriate parser for a file extension
    /// </summary>
    public IFileParser? GetParser(string fileExtension)
    {
        return _parsers.FirstOrDefault(p => p.CanParse(fileExtension));
    }

    /// <summary>
    /// Gets the appropriate importer for a table name
    /// </summary>
    public ITableImporter? GetImporter(string tableName)
    {
        return _importers.FirstOrDefault(i => 
            i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all available table importers
    /// </summary>
    public IEnumerable<ITableImporter> GetAvailableImporters()
    {
        return _importers;
    }

    /// <summary>
    /// Parses a file and returns the raw data
    /// </summary>
    public async Task<IEnumerable<Dictionary<string, object>>> ParseFileAsync(Stream fileStream, string fileExtension)
    {
        var parser = GetParser(fileExtension);
        if (parser == null)
        {
            throw new NotSupportedException($"File type {fileExtension} is not supported.");
        }

        return await parser.ParseAsync(fileStream);
    }

    /// <summary>
    /// Imports data from a parsed file into a database table
    /// </summary>
    public async Task<ImportResult> ImportDataAsync(
        IEnumerable<Dictionary<string, object>> records,
        ImportConfiguration configuration)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ImportResult();
        var recordsList = records.ToList();
        result.TotalRecords = recordsList.Count;

        var importer = GetImporter(configuration.TargetTable);
        if (importer == null)
        {
            result.Errors.Add($"No importer found for table: {configuration.TargetTable}");
            result.FailedRecords = result.TotalRecords;
            result.Duration = stopwatch.Elapsed;
            return result;
        }

        foreach (var record in recordsList)
        {
            try
            {
                // Apply field mappings
                var mappedRecord = ApplyFieldMappings(record, configuration.FieldMappings);

                // Validate if configured
                if (configuration.ValidateBeforeImport)
                {
                    var (isValid, errorMessage) = await importer.ValidateRecordAsync(mappedRecord);
                    if (!isValid)
                    {
                        var recordResult = new ImportRecordResult
                        {
                            Success = false,
                            ErrorMessage = errorMessage,
                            SourceData = record
                        };
                        result.RecordResults.Add(recordResult);
                        result.FailedRecords++;
                        
                        if (!configuration.ContinueOnError)
                        {
                            result.Errors.Add($"Validation failed: {errorMessage}");
                            break;
                        }
                        continue;
                    }
                }

                // Import the record
                var importResult = await importer.ImportRecordAsync(mappedRecord, configuration);
                result.RecordResults.Add(importResult);

                if (importResult.Success)
                {
                    result.SuccessfulRecords++;
                }
                else
                {
                    result.FailedRecords++;
                    if (!configuration.ContinueOnError)
                    {
                        result.Errors.Add($"Import failed: {importResult.ErrorMessage}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                var recordResult = new ImportRecordResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    SourceData = record
                };
                result.RecordResults.Add(recordResult);
                result.FailedRecords++;
                result.Errors.Add($"Unexpected error: {ex.Message}");

                if (!configuration.ContinueOnError)
                {
                    break;
                }
            }
        }

        stopwatch.Stop();
        result.Duration = stopwatch.Elapsed;
        return result;
    }

    /// <summary>
    /// Applies field mappings to transform source data to destination format
    /// </summary>
    private Dictionary<string, object> ApplyFieldMappings(
        Dictionary<string, object> sourceRecord,
        List<FieldMapping> mappings)
    {
        var mappedRecord = new Dictionary<string, object>();

        foreach (var mapping in mappings)
        {
            object? value = null;

            // Get value from source field
            if (sourceRecord.TryGetValue(mapping.SourceField, out var sourceValue))
            {
                value = sourceValue;
            }
            else if (mapping.DefaultValue != null)
            {
                value = mapping.DefaultValue;
            }
            else if (mapping.IsRequired)
            {
                throw new InvalidOperationException(
                    $"Required field '{mapping.SourceField}' is missing and has no default value.");
            }

            // Apply transformation if specified
            if (value != null && mapping.Transform != null)
            {
                value = mapping.Transform(value);
            }

            if (value != null)
            {
                mappedRecord[mapping.DestinationField] = value;
            }
        }

        return mappedRecord;
    }

    /// <summary>
    /// Gets available fields from a sample of parsed records
    /// </summary>
    public List<string> GetAvailableFields(IEnumerable<Dictionary<string, object>> records)
    {
        var fields = new HashSet<string>();
        
        foreach (var record in records.Take(10)) // Sample first 10 records
        {
            foreach (var key in record.Keys)
            {
                fields.Add(key);
            }
        }

        return fields.OrderBy(f => f).ToList();
    }
}
