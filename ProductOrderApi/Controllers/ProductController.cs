using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductOrderApi.Data;
using ProductOrderApi.Models;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

  
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Get All Products
    [HttpGet("get-all-products")]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // 2. Create Product 
    [HttpPost("create-product")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto model)
    {
        Console.WriteLine($"Is User in 'Admin' Role? {User.IsInRole("Admin")}");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Current Claim -> Type: {claim.Type}, Value: {claim.Value}");
        }

        var product = new Product
        {

            Name = model.Name,
            Price = model.Price,
            Description = model.Description,
            Stock = model.Stock
        };

        
        await _context.Products.AddAsync(product);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Product created and saved to database successfully by Admin!",
            product = product
        });
    }

    // 3. Update Product 
    [HttpPatch("update-product/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PatchProduct(int id, [FromBody] UpdateProductDto model)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        
        if (model.Name != null)
        {
            product.Name = model.Name;
        }
        if (model.Price.HasValue)
        {
            product.Price = model.Price.Value;
        }
        if (model.Description != null)
        {
            product.Description = model.Description;
        }
        if (model.Stock.HasValue)
        {
            product.Stock = model.Stock.Value;
        }

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Product with ID {id} patched successfully!",
            product = product
        });
    }

    // 3. Get Product By ID 
    [HttpGet("get-product-by/{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
       
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found." });
        }

        return Ok(product);
    }
}