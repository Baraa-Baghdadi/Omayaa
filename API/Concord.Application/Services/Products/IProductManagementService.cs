using Concord.Application.DTO.Product;
using Concord.Domain.Models.Categories;

namespace Concord.Application.Services.Products
{
    /// <summary>
    /// Service interface for product management operations
    /// </summary>
    public interface IProductManagementService
    {
        /// <summary>
        /// Gets all products with advanced filtering, sorting and pagination
        /// </summary>
        /// <param name="request">Request parameters for filtering and pagination</param>
        /// <returns>Paginated list of products with detailed information</returns>
        Task<GetProductsResponseDto> GetAllProductsAsync(GetProductsRequestDto request);

        /// <summary>
        /// Gets all products for special category ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>list of products with detailed information</returns>
        Task<List<ProductDto>> GetAllProductsAsync(Guid categoryId);

        /// <summary>
        /// Gets a specific product by ID
        /// </summary>
        /// <param name="productId">Product unique identifier</param>
        /// <returns>Product details</returns>
        Task<ProductDto?> GetProductByIdAsync(Guid productId);

        /// <summary>
        /// Creates a new product with optional image upload
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product</returns>
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);

        /// <summary>
        /// Updates an existing product with optional image upload
        /// </summary>
        /// <param name="productId">Product ID to update</param>
        /// <param name="updateProductDto">Product update data</param>
        /// <returns>Updated product</returns>
        Task<ProductDto?> UpdateProductAsync(Guid productId, UpdateProductDto updateProductDto);

        /// <summary>
        /// Deletes a product and its associated image
        /// </summary>
        /// <param name="productId">Product ID to delete</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteProductAsync(Guid productId);

        /// <summary>
        /// Gets product statistics for dashboard
        /// </summary>
        /// <returns>Product statistics</returns>
        Task<ProductStatisticsDto> GetProductStatisticsAsync();

        /// <summary>
        /// Checks if product name exists in the same category
        /// </summary>
        /// <param name="name">Product name</param>
        /// <param name="categoryId">Category ID</param>
        /// <param name="excludeId">Product ID to exclude from check (for updates)</param>
        /// <returns>True if name exists in category</returns>
        Task<bool> ProductNameExistsInCategoryAsync(string name, Guid categoryId, Guid? excludeId = null);
    }
}
