using Concord.Domain.Enums;

namespace Concord.Application.DTO.Orders
{
    public class UpdateOrderStatus
    {
        public Guid OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}
