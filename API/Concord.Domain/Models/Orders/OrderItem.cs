using Concord.Domain.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models.Orders
{
    public class OrderItem : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public Guid ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [MaxLength(150)]
        public string ProductName { get; set; } // Store product name at time of order

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0")]
        public decimal UnitPrice { get; set; } // Store price at time of order

        [Range(0, double.MaxValue, ErrorMessage = "Total price must be greater than or equal to 0")]
        public decimal TotalPrice { get; set; } // Quantity * UnitPrice

        [MaxLength(200)]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
