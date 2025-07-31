namespace Concord.Application.DTO.Orders
{
    public class GetOrdersRequestDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = "";
        public Guid? ProviderId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "OrderDate"; // OrderDate, CustomerName, TotalAmount, Status
        public string SortDirection { get; set; } = "desc"; // asc, desc
        public bool IncludeOrderItems { get; set; } = true;
    }
}
