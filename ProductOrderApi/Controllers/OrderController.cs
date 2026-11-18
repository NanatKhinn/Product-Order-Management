using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductOrderApi.Data;
using ProductOrderApi.Models;
using System.Security.Claims;

namespace ProductOrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        //create order
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto model)
        {  
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new { message = "Authorization header missing or invalid." });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

          
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found inside the token payload." });
            }

            if (model.OrderItems == null || !model.OrderItems.Any())
            {
                return BadRequest(new { message = "Order must contain at least one item." });
            }

            decimal totalAmount = 0;
            var orderItemsList = new List<OrderItem>();

            foreach (var itemDto in model.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {itemDto.ProductId} not found." });
                }

                if (product.Stock < itemDto.Quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock for product '{product.Name}'. Available stock: {product.Stock}" });
                }

                product.Stock -= itemDto.Quantity;
                var itemTotal = product.Price * itemDto.Quantity;
                totalAmount += itemTotal;

                orderItemsList.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                });
            }

            var order = new Order
            {
                UserId = userId, 
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
               
                OrderItems = orderItemsList
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Order created and saved to database successfully!",
                orderId = order.Id,
                totalAmount = order.TotalAmount,
                order
            });
        }

        // 4. Get All Orders for Admin 
        [HttpGet("get-all-orders")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> GetAllOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var totalRecords = await _context.Orders.CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

           

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = orders
            });
        }
        // 2. Get User Orders 
        [HttpGet("get-user-orders")]
        public async Task<IActionResult> GetUserOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

           
            var totalRecords = await _context.Orders
                .Where(o => o.UserId == userId)
                .CountAsync();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate) 
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 3. Pagination Metadata 
            return Ok(new
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = orders
            });
        }
        // 3. Get Order By ID 
        [HttpGet("get-order-by/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = $"Order with ID {id} not found." });
            }

           
            if (order.UserId != userId && !User.IsInRole("Admin")) { return Forbid(); }

            return Ok(order);
        }
    }
    
}