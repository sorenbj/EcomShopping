namespace EcomShopping.FileImport.Core;

/// <summary>
/// Interface for importing data into specific database tables
/// </summary>
public interface ITableImporter
{
    /// <summary>
    /// Gets the name of the table this importer handles
    /// </summary>
    string TableName { get; }
    
    /// <summary>
    /// Validates a record before import
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateRecordAsync(Dictionary<string, object> record);
    
    /// <summary>
    /// Imports a single record into the database
    /// </summary>
    Task<ImportRecordResult> ImportRecordAsync(Dictionary<string, object> record, ImportConfiguration configuration);
    
    /// <summary>
    /// Gets the default field mappings for this table
    /// </summary>
    List<FieldMapping> GetDefaultFieldMappings();
}
