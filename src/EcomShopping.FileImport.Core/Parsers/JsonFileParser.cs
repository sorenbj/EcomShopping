using System.Text.Json;

namespace EcomShopping.FileImport.Core.Parsers;

public class JsonFileParser : IFileParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".json", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ParseAsync(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        var json = await reader.ReadToEndAsync();
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, options);
        return data ?? new List<Dictionary<string, object>>();
    }
}
