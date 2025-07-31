using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Orders
{
    public class UpdateOrderDto
    {

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; } = 0;

        [MaxLength(500)]
        public string Notes { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<UpdateOrderItemDto> OrderItems { get; set; } = new List<UpdateOrderItemDto>();
    }
}
