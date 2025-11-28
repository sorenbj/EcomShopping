using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null, int? categoryId = null);
    Task<Product?> GetBySkuAsync(string sku);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
}
