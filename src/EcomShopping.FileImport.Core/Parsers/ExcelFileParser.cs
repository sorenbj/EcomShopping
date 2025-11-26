using OfficeOpenXml;

namespace EcomShopping.FileImport.Core.Parsers;

public class ExcelFileParser : IFileParser
{
    public ExcelFileParser()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) ||
               fileExtension.Equals(".xls", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ParseAsync(Stream fileStream)
    {
        var result = new List<Dictionary<string, object>>();

        using var package = new ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets[0];
        
        var rowCount = worksheet.Dimension?.Rows ?? 0;
        var colCount = worksheet.Dimension?.Columns ?? 0;

        if (rowCount == 0 || colCount == 0)
            return result;

        // Get headers from first row
        var headers = new List<string>();
        for (int col = 1; col <= colCount; col++)
        {
            headers.Add(worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}");
        }

        // Parse data rows
        for (int row = 2; row <= rowCount; row++)
        {
            var rowData = new Dictionary<string, object>();
            for (int col = 1; col <= colCount; col++)
            {
                var value = worksheet.Cells[row, col].Value;
                rowData[headers[col - 1]] = value ?? string.Empty;
            }
            result.Add(rowData);
        }

        return await Task.FromResult(result);
    }
}
