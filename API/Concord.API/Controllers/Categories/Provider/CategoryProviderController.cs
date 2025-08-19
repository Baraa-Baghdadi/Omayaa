using Concord.Application.Services.Categories;
using Concord.Domain.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Categories.Provider
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Categories for providers")]
    public class CategoryProviderController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryProviderController> _logger;

        public CategoryProviderController(ICategoryService categoryService, ILogger<CategoryProviderController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all categories with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <returns>list categories with detailed information.</returns>
        /// <response code="200">Returns the list of categories successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("GetAllCategories")]
        [SwaggerOperation(
            Summary = "Get all categories",
            Description = "Retrieves all categories. Requires provider role."
        )]
        [ProducesResponseType(typeof(List<Category>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<Category>>> GetAllCategories()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _categoryService.GetAllCategoriesAsync();

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve categories.");
                }

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

    }
}
