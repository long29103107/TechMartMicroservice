﻿namespace TechMart.Services.ProductCatalog.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<string> ImageUrls { get; set; } = new();
    public decimal? Weight { get; set; }
    public string? Brand { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();
}
