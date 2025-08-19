using Concord.Application.DTO.Product;
using Concord.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Products.Provider
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Provider")]
    [SwaggerTag("Products for providers")]
    public class ProductProviderController : ControllerBase
    {
        private readonly IProductManagementService _productManagementService;
        private readonly ILogger<ProductProviderController> _logger;

        public ProductProviderController(IProductManagementService productManagementService,
            ILogger<ProductProviderController> logger)
        {
            _productManagementService = productManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all products with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <param name="categoryID">Category ID</param>
        /// <returns>list of products with detailed information for special category.</returns>
        /// <response code="200">Returns the paginated list of products successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("GetAllProducts")]
        [SwaggerOperation(
            Summary = "Get all products for special category ID",
            Description = "Retrieves all products for special category ID. Requires provider role."
        )]
        [ProducesResponseType(typeof(List<ProductDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<ProductDto>>> GetAllProducts([FromQuery] Guid categoryID)
        {
            try
            {

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _productManagementService.GetAllProductsAsync(categoryID);

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve products.");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters for getting products");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for admin");
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }

    }
}
