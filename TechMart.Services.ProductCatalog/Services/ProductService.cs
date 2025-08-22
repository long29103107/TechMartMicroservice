using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TechMart.Services.ProductCatalog.Data;
using TechMart.Services.ProductCatalog.Models;
using TechMart.Services.ProductCatalog.Services.Interfaces;

namespace TechMart.Services.ProductCatalog.Services;

public class ProductService(
    ProductDbContext context,
    IDistributedCache cache,
    ILogger<ProductService> logger)
    : IProductService
{
    public async Task<Product?> GetProductAsync(int id)
    {
        var cacheKey = $"product_{id}";
        
        // Try cache first
        var cachedProduct = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedProduct))
        {
            return JsonSerializer.Deserialize<Product>(cachedProduct);
        }

        // Get from database
        var product = await context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product != null)
        {
            // Cache for 30 minutes
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
            };
            
            await cache.SetStringAsync(cacheKey, 
                JsonSerializer.Serialize(product), cacheOptions);
        }

        return product;
    }

    public async Task<PagedResult<Product>> GetProductsAsync(ProductSearchCriteria criteria)
    {
        var query = context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .Where(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(criteria.SearchTerm) || 
                                   p.Description.Contains(criteria.SearchTerm));
        }

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
        }

        if (criteria.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
        }

        // Apply sorting
        query = criteria.SortBy?.ToLower() switch
        {
            "name" => criteria.SortDescending ? 
                query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => criteria.SortDescending ? 
                query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "created" => criteria.SortDescending ? 
                query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        
        var products = await query
            .Skip(criteria.Skip)
            .Take(criteria.Take)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = products,
            TotalCount = totalCount,
            PageSize = criteria.Take,
            CurrentPage = (criteria.Skip / criteria.Take) + 1
        };
    }

    public async Task<Product> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            SKU = request.SKU,
            CategoryId = request.CategoryId,
            StockQuantity = request.StockQuantity,
            ImageUrls = request.ImageUrls ?? new List<string>(),
            Weight = request.Weight,
            Brand = request.Brand,
            Attributes = request.Attributes ?? new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();

        logger.LogInformation("Product created: {ProductId} - {ProductName}", 
            product.Id, product.Name);

        return product;
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        var product = await context.Products.FindAsync(productId);
        if (product == null) return false;

        product.StockQuantity = quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        // Invalidate cache
        await cache.RemoveAsync($"product_{productId}");

        logger.LogInformation("Stock updated for product {ProductId}: {Quantity}", 
            productId, quantity);

        return true;
    }
}
