namespace EcomShopping.FileImport.Core;

public interface IFileParser
{
    Task<IEnumerable<Dictionary<string, object>>> ParseAsync(Stream fileStream);
    bool CanParse(string fileExtension);
}
