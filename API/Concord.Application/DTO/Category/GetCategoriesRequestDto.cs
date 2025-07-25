using System.ComponentModel.DataAnnotations;


namespace Concord.Application.DTO.Category
{
    public class GetCategoriesRequestDto
    {
        /// <summary>
        /// Page number (starting from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "رقم الصفحة يجب أن يكون أكبر من 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (1-100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "حجم الصفحة يجب أن يكون بين 1 و 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term to filter categories by name
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Sort field (Name, CreatedAt, ProductCount)
        /// </summary>
        public string SortBy { get; set; } = "Name";

        /// <summary>
        /// Sort direction (asc, desc)
        /// </summary>
        public string SortDirection { get; set; } = "asc";

        /// <summary>
        /// Include product count in response
        /// </summary>
        public bool IncludeProductCount { get; set; } = true;
    }
}
