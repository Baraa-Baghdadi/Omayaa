namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// Request DTO for getting products with filtering and pagination
    /// </summary>
    public class GetProductsRequestDto
    {
        /// <summary>
        /// Page number for pagination (default: 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (default: 10, max: 100)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term to filter products by name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by category ID
        /// </summary>
        public Guid? CategoryId { get; set; }

        /// <summary>
        /// Filter by active status (null = all, true = active only, false = inactive only)
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Minimum price filter
        /// </summary>
        public int? MinPrice { get; set; }

        /// <summary>
        /// Maximum price filter
        /// </summary>
        public int? MaxPrice { get; set; }

        /// <summary>
        /// Sort field (Name, Price, CreatedAt)
        /// </summary>
        public string SortBy { get; set; } = "CreatedAt";

        /// <summary>
        /// Sort direction (asc, desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";

        /// <summary>
        /// Whether to include category information in response
        /// </summary>
        public bool IncludeCategoryInfo { get; set; } = true;
    }
}
