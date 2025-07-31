namespace Concord.Application.DTO.Orders
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public int TotalItems { get; set; }
    }
}
