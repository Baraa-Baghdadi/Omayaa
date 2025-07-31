using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Orders
{
    public class UpdateOrderItemDto
    {
        public Guid? Id { get; set; } // null for new items

        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [MaxLength(200)]
        public string Notes { get; set; }
    }
}
