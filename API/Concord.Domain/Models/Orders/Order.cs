using Concord.Domain.Enums;
using Concord.Domain.Models.Providers;
using System.ComponentModel.DataAnnotations;

namespace Concord.Domain.Models.Orders
{
    public class Order : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        public Guid ProviderId { get; set; }
        public Provider Provider { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be greater than or equal to 0")]
        public decimal TotalAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be greater than or equal to 0")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Final amount must be greater than or equal to 0")]
        public decimal FinalAmount { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveryDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.New;

        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
