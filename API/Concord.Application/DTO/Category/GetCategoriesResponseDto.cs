using Concord.Application.DTO.Provider;

namespace Concord.Application.DTO.Category
{
    public class GetCategoriesResponseDto
    {
        /// <summary>
        /// List of categories
        /// </summary>
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();

        /// <summary>
        /// Pagination information
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}
