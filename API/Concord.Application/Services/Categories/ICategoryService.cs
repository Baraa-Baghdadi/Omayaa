using Concord.Application.DTO.Category;
using Concord.Domain.Models.Categories;

namespace Concord.Application.Services.Categories
{
    /// <summary>
    /// Service interface for category management operations (Admin only)
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Gets all categories with advanced filtering, sorting and pagination
        /// </summary>
        /// <param name="request">Request parameters for filtering and pagination</param>
        /// <returns>Paginated list of categories with detailed information</returns>
        Task<GetCategoriesResponseDto> GetAllCategoriesAsync(GetCategoriesRequestDto request);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <returns>list of categories with detailed information</returns>
        Task<List<Category>> GetAllCategoriesAsync();

        /// <summary>
        /// Gets a specific category by ID
        /// </summary>
        /// <param name="categoryId">Category unique identifier</param>
        /// <returns>Category details</returns>
        Task<CategoryDto?> GetCategoryByIdAsync(Guid categoryId);

        /// <summary>
        /// Creates a new category
        /// </summary>
        /// <param name="createCategoryDto">Category creation data</param>
        /// <returns>Created category</returns>
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);

        /// <summary>
        /// Updates an existing category
        /// </summary>
        /// <param name="categoryId">Category ID to update</param>
        /// <param name="updateCategoryDto">Category update data</param>
        /// <returns>Updated category</returns>
        Task<CategoryDto?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto updateCategoryDto);

        /// <summary>
        /// Deletes a category
        /// </summary>
        /// <param name="categoryId">Category ID to delete</param>
        /// <returns>Success status</returns>
        Task<bool> DeleteCategoryAsync(Guid categoryId);

        /// <summary>
        /// Gets category statistics for dashboard
        /// </summary>
        /// <returns>Category statistics</returns>
        Task<CategoryStatisticsDto> GetCategoryStatisticsAsync();

        /// <summary>
        /// Checks if category name exists
        /// </summary>
        /// <param name="name">Category name</param>
        /// <param name="excludeId">Category ID to exclude from check (for updates)</param>
        /// <returns>True if name exists</returns>
        Task<bool> CategoryNameExistsAsync(string name, Guid? excludeId = null);
    }
}