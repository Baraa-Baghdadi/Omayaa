using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Orders
{
    public class CreateProviderOrder
    {
        public DateTime? DeliveryDate { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "At least one order item is required")]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }
}
