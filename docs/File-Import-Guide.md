# File Import Engine Documentation

## Overview

The File Import Engine provides a comprehensive solution for importing data from external files (Excel, JSON, XML) into the EcomShopping database. It supports field mapping, validation, and error handling to ensure data integrity.

## Features

### Supported File Formats
- **Excel (.xlsx)**: Import data from Excel spreadsheets
- **JSON (.json)**: Import structured JSON data
- **XML (.xml)**: Import XML-formatted data

### Supported Tables
- **Products**: Import product catalog data
- **Categories**: Import product categories

### Key Capabilities
- ✅ File upload and parsing
- ✅ Automatic field detection
- ✅ Configurable field mappings
- ✅ Data validation before import
- ✅ Error handling and logging
- ✅ Import job tracking and status monitoring
- ✅ Batch processing with progress tracking
- ✅ Extensible architecture for new file types and tables

## Architecture

### Core Components

1. **File Parsers** (`EcomShopping.FileImport.Core`)
   - `IFileParser`: Interface for file parsing
   - `ExcelFileParser`: Parses Excel files using EPPlus
   - `JsonFileParser`: Parses JSON files
   - `XmlFileParser`: Parses XML files

2. **Table Importers** (`EcomShopping.Infrastructure.Importers`)
   - `ITableImporter`: Interface for table-specific import logic
   - `ProductImporter`: Handles product imports with validation
   - `CategoryImporter`: Handles category imports

3. **Import Services**
   - `FileImportService`: Core import logic with field mapping
   - `FileImportOrchestrationService`: Orchestrates the import workflow
   - `ImportJobRepository`: Manages import job persistence

4. **API Controllers** (`FileImportController`)
   - File upload endpoint
   - Import execution endpoint
   - Import job management endpoints

5. **UI Components** (`EcomShopping.Web`)
   - `Imports.razor`: Import management dashboard
   - `FileUploadWizard.razor`: Step-by-step file upload wizard

## Usage

### 1. Admin UI (Recommended)

1. Navigate to `/admin/imports` in the web application
2. Click "Import File" button
3. Select the target table (Products or Categories)
4. Upload your file (.xlsx, .json, or .xml)
5. Review the data preview
6. Click "Complete Import" to process the data

### 2. API Endpoints

#### Upload File
```http
POST /api/fileimport/upload
Content-Type: multipart/form-data

Parameters:
- file: The file to upload
- targetTable: Target table name (e.g., "Products", "Categories")
- createdBy: (Optional) User who created the import
```

Response:
```json
{
  "jobId": 1,
  "fileName": "products.xlsx",
  "totalRecords": 100,
  "availableFields": ["Name", "SKU", "Price", "Description"],
  "previewRecords": [...]
}
```

#### Get Available Tables
```http
GET /api/fileimport/tables
```

Response:
```json
[
  {
    "tableName": "Products",
    "defaultFieldMappings": [
      {
        "sourceField": "Name",
        "destinationField": "Name",
        "isRequired": true
      },
      ...
    ]
  }
]
```

#### Get Import Jobs
```http
GET /api/fileimport
GET /api/fileimport/recent?count=10
GET /api/fileimport/{id}
GET /api/fileimport/status/{status}
```

## File Format Examples

### Products - JSON
```json
[
  {
    "Name": "Wireless Keyboard",
    "SKU": "KB-WL-001",
    "Description": "Compact wireless keyboard",
    "Price": "49.99",
    "StockQuantity": "150",
    "CategoryName": "Electronics",
    "IsActive": "true",
    "ImageUrl": "https://example.com/image.jpg"
  }
]
```

### Products - XML
```xml
<?xml version="1.0" encoding="UTF-8"?>
<products>
    <product>
        <Name>Wireless Keyboard</Name>
        <SKU>KB-WL-001</SKU>
        <Description>Compact wireless keyboard</Description>
        <Price>49.99</Price>
        <StockQuantity>150</StockQuantity>
        <CategoryName>Electronics</CategoryName>
        <IsActive>true</IsActive>
        <ImageUrl>https://example.com/image.jpg</ImageUrl>
    </product>
</products>
```

### Products - Excel (.xlsx)
Create an Excel file with headers in the first row:

| Name | SKU | Description | Price | StockQuantity | CategoryName | IsActive | ImageUrl |
|------|-----|-------------|-------|---------------|--------------|----------|----------|
| Wireless Keyboard | KB-WL-001 | Compact wireless keyboard | 49.99 | 150 | Electronics | true | https://example.com/image.jpg |

### Categories - JSON
```json
[
  {
    "Name": "Electronics",
    "Description": "Electronic devices and accessories",
    "ParentCategoryName": ""
  }
]
```

## Field Mappings

### Products Table

| Source Field | Destination Field | Required | Type | Notes |
|--------------|-------------------|----------|------|-------|
| Name | Name | Yes | string | Product name |
| SKU | SKU | Yes | string | Unique product identifier |
| Description | Description | No | string | Product description |
| Price | Price | Yes | decimal | Product price |
| StockQuantity | StockQuantity | No | int | Initial stock (default: 0) |
| CategoryName | CategoryName | No | string | Category name (must exist) |
| IsActive | IsActive | No | bool | Active status (default: true) |
| ImageUrl | ImageUrl | No | string | Product image URL |

**Additional Fields**: Any additional fields not in the standard list will be stored in the product's `Metadata` property as key-value pairs.

### Categories Table

| Source Field | Destination Field | Required | Type | Notes |
|--------------|-------------------|----------|------|-------|
| Name | Name | Yes | string | Category name (must be unique) |
| Description | Description | No | string | Category description |
| ParentCategoryName | ParentCategoryName | No | string | Parent category name (must exist) |

## Validation Rules

### Products
- **Name**: Required, max 200 characters
- **SKU**: Required, max 100 characters, must be unique
- **Price**: Required, must be a positive decimal number
- **StockQuantity**: Must be a non-negative integer
- **Category**: If provided, must exist in the database

### Categories
- **Name**: Required, max 200 characters, must be unique
- **Parent Category**: If provided, must exist in the database

## Error Handling

The import engine provides comprehensive error handling:

1. **Validation Errors**: Field-level validation before import
2. **Parsing Errors**: Invalid file format or structure
3. **Database Errors**: Constraint violations, duplicate keys
4. **Import Job Tracking**: All errors are logged to the ImportJob.ErrorLog field

### Import Job Status
- **Pending**: Job created, awaiting processing
- **Processing**: Import in progress
- **Completed**: All records imported successfully
- **PartiallyCompleted**: Some records failed, some succeeded
- **Failed**: Import failed completely

## Extending the Import Engine

### Adding a New File Type

1. Create a new parser implementing `IFileParser`:
```csharp
public class CsvFileParser : IFileParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ParseAsync(Stream fileStream)
    {
        // Implementation
    }
}
```

2. Register the parser in `Program.cs`:
```csharp
builder.Services.AddScoped<IFileParser, CsvFileParser>();
```

### Adding a New Table Importer

1. Create a new importer implementing `ITableImporter`:
```csharp
public class OrderImporter : ITableImporter
{
    public string TableName => "Orders";

    public List<FieldMapping> GetDefaultFieldMappings()
    {
        // Define field mappings
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateRecordAsync(
        Dictionary<string, object> record)
    {
        // Validation logic
    }

    public async Task<ImportRecordResult> ImportRecordAsync(
        Dictionary<string, object> record,
        ImportConfiguration configuration)
    {
        // Import logic
    }
}
```

2. Register the importer in `Program.cs`:
```csharp
builder.Services.AddScoped<ITableImporter, OrderImporter>();
```

## Performance Considerations

- **File Size Limit**: Default 10MB per file (configurable in `FileUploadWizard.razor`)
- **Batch Processing**: Records are processed sequentially with error recovery
- **Memory Usage**: Files are streamed to minimize memory footprint
- **Database Transactions**: Each record is saved individually (can be optimized for batch inserts)

## Security Considerations

- **File Type Validation**: Only allowed file types can be uploaded
- **Size Limits**: Enforced at both client and server level
- **Data Validation**: All data is validated before import
- **SQL Injection Protection**: Entity Framework parameterization prevents SQL injection
- **XSS Protection**: Data is sanitized through model binding

## Troubleshooting

### Common Issues

1. **"File type not supported"**
   - Ensure the file has the correct extension (.xlsx, .json, .xml)
   - Check that the file is not corrupted

2. **"Validation failed"**
   - Check that all required fields are present
   - Verify data types match expected formats
   - Ensure unique constraints are not violated (SKU, Category Name)

3. **"Category not found"**
   - Import categories before products if using CategoryName
   - Ensure category names match exactly (case-insensitive)

4. **Import partially completed**
   - Check the error log for specific record failures
   - Fix the data and re-import failed records

## Best Practices

1. **Data Preparation**
   - Clean and validate data before import
   - Ensure unique identifiers (SKU, Name) are truly unique
   - Use consistent data formats (dates, numbers)

2. **Import Order**
   - Import parent entities before children (Categories before Products)
   - Start with small batches to test field mappings

3. **Error Recovery**
   - Review error logs for failed imports
   - Fix data issues and re-import
   - Use the "Continue on Error" option for large imports

4. **Testing**
   - Test with sample data first
   - Verify imported data in the database
   - Check for data integrity and relationships

## Sample Files

Sample import files are available in the `/docs/samples/` directory:
- `products.json` - JSON format product import
- `products.xml` - XML format product import
- `categories.json` - JSON format category import

## API Integration

For programmatic access, see the [API Documentation](API.md) for detailed endpoint specifications and authentication requirements.
