using Concord.Application.DTO.Provider;

namespace Concord.Application.DTO.Product
{
    /// <summary>
    /// Response DTO for getting products with pagination
    /// </summary>
    public class GetProductsResponseDto
    {
        /// <summary>
        /// List of products
        /// </summary>
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();

        /// <summary>
        /// Pagination information
        /// </summary>
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }
}
