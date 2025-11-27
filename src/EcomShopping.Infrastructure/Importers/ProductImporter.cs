using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.FileImport.Core;

namespace EcomShopping.Infrastructure.Importers;

/// <summary>
/// Importer for Product table
/// </summary>
public class ProductImporter : ITableImporter
{
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductImporter(
        IProductRepository productRepository,
        IRepository<Category> categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public string TableName => "Products";

    public List<FieldMapping> GetDefaultFieldMappings()
    {
        return new List<FieldMapping>
        {
            new() { SourceField = "Name", DestinationField = "Name", IsRequired = true },
            new() { SourceField = "SKU", DestinationField = "SKU", IsRequired = true },
            new() { SourceField = "Description", DestinationField = "Description", IsRequired = false },
            new() { SourceField = "Price", DestinationField = "Price", IsRequired = true, 
                Transform = obj => Convert.ToDecimal(obj.ToString()) },
            new() { SourceField = "StockQuantity", DestinationField = "StockQuantity", IsRequired = false, 
                DefaultValue = "0",
                Transform = obj => Convert.ToInt32(obj.ToString()) },
            new() { SourceField = "CategoryName", DestinationField = "CategoryName", IsRequired = false },
            new() { SourceField = "IsActive", DestinationField = "IsActive", IsRequired = false,
                DefaultValue = "true",
                Transform = obj => Convert.ToBoolean(obj.ToString()) },
            new() { SourceField = "ImageUrl", DestinationField = "ImageUrl", IsRequired = false },
        };
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateRecordAsync(Dictionary<string, object> record)
    {
        // Validate required fields
        if (!record.ContainsKey("Name") || string.IsNullOrWhiteSpace(record["Name"]?.ToString()))
        {
            return (false, "Product name is required");
        }

        if (!record.ContainsKey("SKU") || string.IsNullOrWhiteSpace(record["SKU"]?.ToString()))
        {
            return (false, "Product SKU is required");
        }

        if (!record.ContainsKey("Price"))
        {
            return (false, "Product price is required");
        }

        // Validate price is a valid decimal
        if (!decimal.TryParse(record["Price"]?.ToString(), out var price) || price < 0)
        {
            return (false, "Product price must be a valid positive number");
        }

        // Validate SKU uniqueness
        var sku = record["SKU"].ToString()!;
        var existingProduct = await _productRepository.GetBySkuAsync(sku);
        if (existingProduct != null)
        {
            return (false, $"Product with SKU '{sku}' already exists");
        }

        return (true, null);
    }

    public async Task<ImportRecordResult> ImportRecordAsync(
        Dictionary<string, object> record,
        ImportConfiguration configuration)
    {
        try
        {
            var product = new Product
            {
                Name = record["Name"].ToString()!,
                SKU = record["SKU"].ToString()!,
                Description = record.ContainsKey("Description") 
                    ? record["Description"]?.ToString() ?? string.Empty
                    : string.Empty,
                Price = Convert.ToDecimal(record["Price"].ToString()),
                StockQuantity = record.ContainsKey("StockQuantity") 
                    ? Convert.ToInt32(record["StockQuantity"].ToString())
                    : 0,
                IsActive = record.ContainsKey("IsActive")
                    ? Convert.ToBoolean(record["IsActive"].ToString())
                    : true,
                Slug = GenerateSlug(record["Name"].ToString()!)
            };

            // Handle category if provided
            if (record.ContainsKey("CategoryName") && 
                !string.IsNullOrWhiteSpace(record["CategoryName"]?.ToString()))
            {
                var categoryName = record["CategoryName"].ToString()!;
                var categories = await _categoryRepository.GetAllAsync();
                var category = categories.FirstOrDefault(c => 
                    c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                
                if (category != null)
                {
                    product.CategoryId = category.Id;
                }
            }

            // Handle image URL if provided
            if (record.ContainsKey("ImageUrl") && 
                !string.IsNullOrWhiteSpace(record["ImageUrl"]?.ToString()))
            {
                var imageUrl = record["ImageUrl"].ToString()!;
                product.Images = new List<string> { imageUrl };
            }

            // Handle metadata fields (additional custom fields)
            var metadata = new Dictionary<string, string>();
            foreach (var kvp in record)
            {
                // Add fields that aren't standard product fields to metadata
                if (!IsStandardField(kvp.Key))
                {
                    metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
            }
            if (metadata.Any())
            {
                product.Metadata = metadata;
            }

            var createdProduct = await _productRepository.AddAsync(product);

            return new ImportRecordResult
            {
                Success = true,
                SourceData = record,
                ImportedEntity = createdProduct
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

    private bool IsStandardField(string fieldName)
    {
        var standardFields = new[] 
        { 
            "Name", "SKU", "Description", "Price", "StockQuantity", 
            "CategoryName", "IsActive", "ImageUrl", "Slug" 
        };
        return standardFields.Contains(fieldName, StringComparer.OrdinalIgnoreCase);
    }

    private string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
        
        // Remove special characters
        slug = new string(slug.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
        
        // Remove duplicate dashes
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }
        
        return slug.Trim('-');
    }
}
