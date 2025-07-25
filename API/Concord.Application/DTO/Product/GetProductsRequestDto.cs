using System.ComponentModel.DataAnnotations;


namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// DTO for product list requests with filtering and pagination
    /// </summary>
    public class GetProductsRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public string SearchTerm { get; set; }

        public Guid? CategoryId { get; set; }

        public int? MinPrice { get; set; }

        public int? MaxPrice { get; set; }

        public bool? IsActive { get; set; }

        public bool? HasDiscount { get; set; }

        public string SortBy { get; set; } = "Name"; // Name, Price, CreatedAt, UpdatedAt

        public string SortOrder { get; set; } = "ASC"; // ASC, DESC

        public DateTime? CreatedAfter { get; set; }

        public DateTime? CreatedBefore { get; set; }
    }
}
