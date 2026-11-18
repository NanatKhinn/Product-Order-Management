using System.ComponentModel.DataAnnotations;

namespace ProductOrderApi.Models
{
    public class ProductDto
    {
        public string Name { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
        public string Description { get; set; }
    }

    public class UpdateProductDto
    {
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public int? Stock { get; set; }
    }
}
