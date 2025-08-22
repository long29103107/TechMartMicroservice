using TechMart.Services.ProductCatalog.Models;

namespace TechMart.Services.ProductCatalog.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<Product>> GetProductsAsync(ProductSearchCriteria criteria);
    Task<Product?> GetProductAsync(int id);
    Task<Product> CreateProductAsync(CreateProductRequest request);
    Task<Product> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> UpdateStockAsync(int productId, int quantity);
}