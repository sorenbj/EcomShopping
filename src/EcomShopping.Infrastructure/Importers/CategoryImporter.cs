using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.FileImport.Core;

namespace EcomShopping.Infrastructure.Importers;

/// <summary>
/// Importer for Category table
/// </summary>
public class CategoryImporter : ITableImporter
{
    private readonly IRepository<Category> _categoryRepository;

    public CategoryImporter(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public string TableName => "Categories";

    public List<FieldMapping> GetDefaultFieldMappings()
    {
        return new List<FieldMapping>
        {
            new() { SourceField = "Name", DestinationField = "Name", IsRequired = true },
            new() { SourceField = "Description", DestinationField = "Description", IsRequired = false },
            new() { SourceField = "ParentCategoryName", DestinationField = "ParentCategoryName", IsRequired = false },
        };
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateRecordAsync(Dictionary<string, object> record)
    {
        // Validate required fields
        if (!record.ContainsKey("Name") || string.IsNullOrWhiteSpace(record["Name"]?.ToString()))
        {
            return (false, "Category name is required");
        }

        // Validate name uniqueness
        var name = record["Name"].ToString()!;
        var categories = await _categoryRepository.GetAllAsync();
        var existingCategory = categories.FirstOrDefault(c => 
            c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        
        if (existingCategory != null)
        {
            return (false, $"Category with name '{name}' already exists");
        }

        return (true, null);
    }

    public async Task<ImportRecordResult> ImportRecordAsync(
        Dictionary<string, object> record,
        ImportConfiguration configuration)
    {
        try
        {
            var category = new Category
            {
                Name = record["Name"].ToString()!,
                Description = record.ContainsKey("Description") 
                    ? record["Description"]?.ToString() ?? string.Empty
                    : string.Empty
            };

            // Handle parent category if provided
            if (record.ContainsKey("ParentCategoryName") && 
                !string.IsNullOrWhiteSpace(record["ParentCategoryName"]?.ToString()))
            {
                var parentCategoryName = record["ParentCategoryName"].ToString()!;
                var categories = await _categoryRepository.GetAllAsync();
                var parentCategory = categories.FirstOrDefault(c => 
                    c.Name.Equals(parentCategoryName, StringComparison.OrdinalIgnoreCase));
                
                if (parentCategory != null)
                {
                    category.ParentCategoryId = parentCategory.Id;
                }
            }

            var createdCategory = await _categoryRepository.AddAsync(category);

            return new ImportRecordResult
            {
                Success = true,
                SourceData = record,
                ImportedEntity = createdCategory
            };
        }
        catch (Exception ex)
        {
            return new ImportRecordResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                SourceData = record
            };
        }
    }
}
