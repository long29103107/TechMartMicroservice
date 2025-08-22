using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TechMart.Services.ProductCatalog.Models;

namespace TechMart.Services.ProductCatalog.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ImageUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            entity.Property(e => e.Attributes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null));
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId);
        });
    }
}