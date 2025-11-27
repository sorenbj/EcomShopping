using System.Xml.Linq;

namespace EcomShopping.FileImport.Core.Parsers;

public class XmlFileParser : IFileParser
{
    public bool CanParse(string fileExtension)
    {
        return fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Dictionary<string, object>>> ParseAsync(Stream fileStream)
    {
        var result = new List<Dictionary<string, object>>();

        using var reader = new StreamReader(fileStream);
        var xmlContent = await reader.ReadToEndAsync();
        
        var doc = XDocument.Parse(xmlContent);
        var root = doc.Root;
        
        if (root == null)
            return result;

        // Try to find collection elements (common patterns: items, records, products, etc.)
        var itemElements = root.Elements().ToList();
        
        // If there are no direct children, return empty
        if (!itemElements.Any())
            return result;

        // Check if all elements have the same name (indicating they are collection items)
        var firstElementName = itemElements[0].Name;
        var allSameName = itemElements.All(e => e.Name == firstElementName);

        // If they all have the same name and have child elements or attributes, they are likely the data records
        if (allSameName && itemElements.All(e => e.HasElements || e.HasAttributes))
        {
            // These are the data records
        }
        // Otherwise, check if root has a single wrapper element
        else if (itemElements.Count == 1 && itemElements[0].HasElements)
        {
            itemElements = itemElements[0].Elements().ToList();
        }

        foreach (var item in itemElements)
        {
            var rowData = new Dictionary<string, object>();
            
            // Parse all child elements as key-value pairs
            foreach (var element in item.Elements())
            {
                var key = element.Name.LocalName;
                var value = element.Value;
                
                rowData[key] = value ?? string.Empty;
            }
            
            // Parse attributes as well
            foreach (var attribute in item.Attributes())
            {
                var key = attribute.Name.LocalName;
                var value = attribute.Value;
                
                rowData[key] = value ?? string.Empty;
            }
            
            if (rowData.Any())
            {
                result.Add(rowData);
            }
        }

        return result;
    }
}
