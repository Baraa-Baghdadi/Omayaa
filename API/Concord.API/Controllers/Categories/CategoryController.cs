using Concord.Application.DTO.Category;
using Concord.Application.Services.Categories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Categories
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Category Management - Admin operations for managing product categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all categories with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of categories with detailed information.</returns>
        /// <response code="200">Returns the paginated list of categories successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all categories with pagination and filtering",
            Description = "Retrieves all categories with advanced filtering options, sorting, and pagination. Requires admin role."
        )]
        [ProducesResponseType(typeof(GetCategoriesResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetCategoriesResponseDto>> GetAllCategories([FromQuery] GetCategoriesRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin user requesting categories list with filters: {@Request}", request);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryService.GetAllCategoriesAsync(request);

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve categories.");
                }

                _logger.LogInformation("Successfully retrieved {Count} categories for admin", result.Categories.Count);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters provided");
                return BadRequest("Invalid request parameters provided.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to category management");
                return Forbid("Insufficient permissions to view categories.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving categories");
                return StatusCode(500, "An error occurred while retrieving categories.");
            }
        }

        /// <summary>
        /// Retrieves a specific category by ID (Admin only).
        /// </summary>
        /// <param name="id">Category unique identifier</param>
        /// <returns>Category details.</returns>
        /// <response code="200">Returns the category details successfully.</response>
        /// <response code="400">Invalid category ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get category by ID",
            Description = "Retrieves detailed information about a specific category by its unique identifier."
        )]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid category ID provided.");
                }

                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                _logger.LogInformation("Admin retrieved category details for ID: {CategoryId}", id);
                return Ok(category);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category ID format: {CategoryId}", id);
                return BadRequest("Invalid category ID format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category {CategoryId}", id);
                return StatusCode(500, "An error occurred while retrieving category details.");
            }
        }

        /// <summary>
        /// Creates a new category (Admin only).
        /// </summary>
        /// <param name="createCategoryDto">Category creation data</param>
        /// <returns>Created category details.</returns>
        /// <response code="201">Category created successfully.</response>
        /// <response code="400">Invalid category data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="409">Category name already exists.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new category",
            Description = "Creates a new product category with the specified name."
        )]
        [ProducesResponseType(typeof(CategoryDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdCategory = await _categoryService.CreateCategoryAsync(createCategoryDto);

                _logger.LogInformation("Admin created new category: {CategoryName} with ID: {CategoryId}",
                    createdCategory.Name, createdCategory.Id);

                return CreatedAtAction(
                    nameof(GetCategoryById),
                    new { id = createdCategory.Id },
                    createdCategory);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("موجود مسبقاً"))
            {
                _logger.LogWarning("Attempt to create category with existing name: {CategoryName}", createCategoryDto.Name);
                return Conflict("Category name already exists.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category data provided");
                return BadRequest("Invalid category data provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating category");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing category (Admin only).
        /// </summary>
        /// <param name="id">Category ID to update</param>
        /// <param name="updateCategoryDto">Category update data</param>
        /// <returns>Updated category details.</returns>
        /// <response code="200">Category updated successfully.</response>
        /// <response code="400">Invalid category data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="409">Category name already exists.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an existing category",
            Description = "Updates the details of an existing category by its unique identifier."
        )]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid category ID provided.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

                if (updatedCategory == null)
                {
                    return NotFound("Category not found.");
                }

                _logger.LogInformation("Admin updated category: {CategoryId} with new name: {CategoryName}",
                    id, updatedCategory.Name);

                return Ok(updatedCategory);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("موجود مسبقاً"))
            {
                _logger.LogWarning("Attempt to update category {CategoryId} with existing name: {CategoryName}",
                    id, updateCategoryDto.Name);
                return Conflict("Category name already exists.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category data provided for update");
                return BadRequest("Invalid category data provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category {CategoryId}", id);
                return StatusCode(500, "An error occurred while updating the category.");
            }
        }

        /// <summary>
        /// Deletes a category (Admin only).
        /// </summary>
        /// <param name="id">Category ID to delete</param>
        /// <returns>Success status of the deletion operation.</returns>
        /// <response code="200">Category deleted successfully.</response>
        /// <response code="400">Invalid category ID or category has associated products.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Category not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a category",
            Description = "Deletes an existing category by its unique identifier. Category must not have associated products."
        )]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> DeleteCategory(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid category ID provided.");
                }

                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                {
                    return NotFound("Category not found.");
                }

                _logger.LogInformation("Admin deleted category: {CategoryId}", id);
                return Ok(true);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("يحتوي على منتجات"))
            {
                _logger.LogWarning("Attempt to delete category {CategoryId} that contains products", id);
                return BadRequest("Cannot delete category because it contains products.");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category ID provided for deletion: {CategoryId}", id);
                return BadRequest("Invalid category ID provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category {CategoryId}", id);
                return StatusCode(500, "An error occurred while deleting the category.");
            }
        }

        /// <summary>
        /// Retrieves category statistics for dashboard (Admin only).
        /// </summary>
        /// <returns>Category statistics including counts and trends.</returns>
        /// <response code="200">Returns category statistics successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("statistics")]
        [SwaggerOperation(
            Summary = "Get category statistics",
            Description = "Retrieves comprehensive statistics about categories for admin dashboard."
        )]
        [ProducesResponseType(typeof(CategoryStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<CategoryStatisticsDto>> GetCategoryStatistics()
        {
            try
            {
                var statistics = await _categoryService.GetCategoryStatisticsAsync();

                _logger.LogInformation("Admin retrieved category statistics: Total={Total}, WithProducts={WithProducts}",
                    statistics.TotalCategories, statistics.CategoriesWithProducts);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category statistics");
                return StatusCode(500, "An error occurred while retrieving category statistics.");
            }
        }

        /// <summary>
        /// Checks if a category name exists (Admin only).
        /// </summary>
        /// <param name="name">Category name to check</param>
        /// <param name="excludeId">Category ID to exclude from check (optional, for updates)</param>
        /// <returns>Boolean indicating if name exists.</returns>
        /// <response code="200">Returns existence check result successfully.</response>
        /// <response code="400">Invalid category name provided.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("check-name")]
        [SwaggerOperation(
            Summary = "Check if category name exists",
            Description = "Checks if a category name already exists in the system. Useful for validation during create/update operations."
        )]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> CheckCategoryNameExists([FromQuery] string name, [FromQuery] Guid? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Category name is required.");
                }

                var exists = await _categoryService.CategoryNameExistsAsync(name, excludeId);

                return Ok(exists);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid category name provided for existence check");
                return BadRequest("Invalid category name provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking category name existence");
                return StatusCode(500, "An error occurred while checking category name existence.");
            }
        }
    }
}