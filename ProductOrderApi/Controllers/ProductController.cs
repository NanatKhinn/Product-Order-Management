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

    // Dependency Injection မှတစ်ဆင့် Database Context ကို ခေါ်သုံးခြင်း
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Get All Products (Login ဝင်ထားသူတိုင်း ဝင်ကြည့်နိုင်သည်)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProducts()
    {
        // Database ထဲရှိ Product အားလုံးကို ဆွဲထုတ်ခြင်း
        var products = await _context.Products.ToListAsync();
        return Ok(products);
    }

    // 2. Create Product (Admin Role ရှိသူသာ Product အသစ် ထည့်နိုင်သည်)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto model)
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"CLAIM TYPE: {claim.Type} | VALUE: {claim.Value}");
        }
        // DTO မှ Model သို့ Data ကူးပြောင်းခြင်း (Mapping)
        var product = new Product
        {

            Name = model.Name,
            Price = model.Price,
            Description = model.Description,
            Stock = model.Stock
        };

        // Database ထဲသို့ အသစ်ထည့်ရန် Add လုပ်ခြင်း
        await _context.Products.AddAsync(product);

        // Database ထဲသို့ အပြီးအပိုင် သိမ်းဆည်းခြင်း (Commit)
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Product created and saved to database successfully by Admin!",
            product = product
        });
    }
}