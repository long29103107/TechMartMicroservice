using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMart.Services.ProductCatalog.Models;
using TechMart.Services.ProductCatalog.Services.Interfaces;

namespace TechMart.Services.ProductCatalog.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Product>>> GetProducts([FromQuery] ProductSearchCriteria criteria)
    {
        var result = await productService.GetProductsAsync(criteria);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}/stock")]
    [Authorize(Roles = "Admin,Vendor")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockRequest request)
    {
        var result = await productService.UpdateStockAsync(id, request.Quantity);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}