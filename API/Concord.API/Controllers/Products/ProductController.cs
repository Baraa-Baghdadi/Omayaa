using Concord.Application.DTO.Product;
using Concord.Application.Services.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Products
{
    /// <summary>
    /// Product management controller for administrative operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Product Management - Admin operations for managing products")]
    public class ProductController : ControllerBase
    {
        private readonly IProductManagementService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductManagementService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all products with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of products with detailed information.</returns>
        /// <response code="200">Returns the paginated list of products successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all products with pagination and filtering",
            Description = "Retrieves all products with advanced filtering options, sorting, and pagination. Requires admin role."
        )]
        [ProducesResponseType(typeof(GetProductsResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetProductsResponseDto>> GetAllProducts([FromQuery] GetProductsRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin user requesting products list with filters: {@Request}", request);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _productService.GetAllProductsAsync(request);

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve products.");
                }

                _logger.LogInformation("Successfully retrieved {Count} products for admin", result.Products.Count);
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

        /// <summary>
        /// Retrieves a specific product by ID (Admin only).
        /// </summary>
        /// <param name="id">Product unique identifier</param>
        /// <returns>Product details.</returns>
        /// <response code="200">Returns the product details successfully.</response>
        /// <response code="400">Invalid product ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get product by ID",
            Description = "Retrieves a specific product by its unique identifier. Requires admin role."
        )]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
        {
            try
            {
                _logger.LogInformation("Admin user requesting product by ID: {ProductId}", id);

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("Product not found: {ProductId}", id);
                    return NotFound($"Product with ID {id} not found.");
                }

                _logger.LogInformation("Successfully retrieved product: {ProductId}", id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by ID: {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product.");
            }
        }

        /// <summary>
        /// Creates a new product with optional image upload (Admin only).
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product details.</returns>
        /// <response code="201">Product created successfully.</response>
        /// <response code="400">Invalid product data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new product",
            Description = "Creates a new product with optional image upload. Only one product can be created per API call. Requires admin role."
        )]
        [ProducesResponseType(typeof(ProductDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            try
            {
                _logger.LogInformation("Admin user creating new product: {ProductName}", createProductDto.Name);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate image if provided
                if (createProductDto.Image != null)
                {
                    var validationResult = ValidateImage(createProductDto.Image);
                    if (!validationResult.IsValid)
                    {
                        return BadRequest(validationResult.ErrorMessage);
                    }
                }

                var result = await _productService.CreateProductAsync(createProductDto);

                _logger.LogInformation("Product created successfully: {ProductId}", result.Id);

                return CreatedAtAction(
                    nameof(GetProductById),
                    new { id = result.Id },
                    result
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data for creating product");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating product");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }

        /// <summary>
        /// Updates an existing product with optional image upload (Admin only).
        /// </summary>
        /// <param name="id">Product ID to update</param>
        /// <param name="updateProductDto">Product update data</param>
        /// <returns>Updated product details.</returns>
        /// <response code="200">Product updated successfully.</response>
        /// <response code="400">Invalid product data or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an existing product",
            Description = "Updates an existing product with optional image upload. Requires admin role."
        )]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromForm] UpdateProductDto updateProductDto)
        {
            try
            {
                _logger.LogInformation("Admin user updating product: {ProductId}", id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate image if provided
                if (updateProductDto.Image != null)
                {
                    var validationResult = ValidateImage(updateProductDto.Image);
                    if (!validationResult.IsValid)
                    {
                        return BadRequest(validationResult.ErrorMessage);
                    }
                }

                var result = await _productService.UpdateProductAsync(id, updateProductDto);

                if (result == null)
                {
                    _logger.LogWarning("Product not found for update: {ProductId}", id);
                    return NotFound($"Product with ID {id} not found.");
                }

                _logger.LogInformation("Product updated successfully: {ProductId}", id);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid data for updating product: {ProductId}", id);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", id);
                return StatusCode(500, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating product: {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        /// <summary>
        /// Deletes a product and its associated image (Admin only).
        /// </summary>
        /// <param name="id">Product ID to delete</param>
        /// <returns>Success status.</returns>
        /// <response code="204">Product deleted successfully.</response>
        /// <response code="400">Invalid product ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Product not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a product",
            Description = "Deletes a product and its associated image file. This action cannot be undone. Requires admin role."
        )]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteProduct(Guid id)
        {
            try
            {
                _logger.LogInformation("Admin user deleting product: {ProductId}", id);

                var result = await _productService.DeleteProductAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Product not found for deletion: {ProductId}", id);
                    return NotFound($"Product with ID {id} not found.");
                }

                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }

        /// <summary>
        /// Gets product statistics for admin dashboard.
        /// </summary>
        /// <returns>Product statistics including counts, averages, and trends.</returns>
        /// <response code="200">Returns product statistics successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("statistics")]
        [SwaggerOperation(
            Summary = "Get product statistics",
            Description = "Retrieves comprehensive product statistics for admin dashboard including totals, averages, and trends. Requires admin role."
        )]
        [ProducesResponseType(typeof(ProductStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductStatisticsDto>> GetProductStatistics()
        {
            try
            {
                _logger.LogInformation("Admin user requesting product statistics");

                var statistics = await _productService.GetProductStatisticsAsync();

                _logger.LogInformation("Successfully retrieved product statistics");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product statistics");
                return StatusCode(500, "An error occurred while retrieving product statistics.");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validates uploaded image file
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateImage(IFormFile image)
        {
            // Check file size (max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (image.Length > maxFileSize)
            {
                return (false, "Image file size cannot exceed 5MB.");
            }

            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "Only image files (jpg, jpeg, png, gif, webp) are allowed.");
            }

            // Check content type
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedContentTypes.Contains(image.ContentType.ToLowerInvariant()))
            {
                return (false, "Invalid image content type.");
            }

            return (true, string.Empty);
        }

        #endregion

    }
}
