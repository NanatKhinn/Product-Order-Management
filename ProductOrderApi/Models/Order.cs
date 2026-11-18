
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductOrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        
        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}