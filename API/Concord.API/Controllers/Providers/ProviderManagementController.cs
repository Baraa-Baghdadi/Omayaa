using Concord.Application.DTO.Identity;
using Concord.Application.DTO.Provider;
using Concord.Application.Services.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Concord.API.Controllers.Providers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [SwaggerTag("Provider Management - Admin operations for managing provider accounts")]
    public class ProviderManagementController : ControllerBase
    {
        private readonly IProviderManagementService _providerManagementService;
        private readonly ILogger<ProviderManagementController> _logger;

        public ProviderManagementController(
            IProviderManagementService providerManagementService,
            ILogger<ProviderManagementController> logger)
        {
            _providerManagementService = providerManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all providers with advanced filtering and pagination (Admin only).
        /// </summary>
        /// <param name="request">Filter and pagination parameters</param>
        /// <returns>Paginated list of providers with detailed information.</returns>
        /// <response code="200">Returns the paginated list of providers successfully.</response>
        /// <response code="400">Invalid request parameters or validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("providers")]
        [SwaggerOperation(
            Summary = "Get all providers with pagination and filtering",
            Description = "Retrieves all providers with advanced filtering options, sorting, and pagination. Requires admin role."
        )]
        [ProducesResponseType(typeof(GetProvidersResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetProvidersResponseDto>> GetAllProviders([FromQuery] GetProvidersRequestDto request)
        {
            try
            {
                _logger.LogInformation("Admin user requesting providers list with filters: {Request}", request);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerManagementService.GetAllProvidersAsync(request);

                if (result == null)
                {
                    return StatusCode(500, "Unable to retrieve providers.");
                }

                _logger.LogInformation("Successfully retrieved {Count} providers for admin", result.Providers.Count);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters provided");
                return BadRequest("Invalid request parameters provided.");
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt to provider management");
                return Forbid("Insufficient permissions to view providers.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving providers");
                return StatusCode(500, "An error occurred while retrieving providers.");
            }
        }

        /// <summary>
        /// Retrieves a specific provider by ID (Admin only).
        /// </summary>
        /// <param name="id">Provider unique identifier</param>
        /// <returns>Provider details.</returns>
        /// <response code="200">Returns the provider details successfully.</response>
        /// <response code="400">Invalid provider ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Provider not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("providers/{id:guid}")]
        [SwaggerOperation(
            Summary = "Get provider by ID",
            Description = "Retrieves detailed information about a specific provider by their unique identifier."
        )]
        [ProducesResponseType(typeof(ProviderManagementDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProviderManagementDto>> GetProviderById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest("Invalid provider ID provided.");
                }

                var provider = await _providerManagementService.GetProviderByIdAsync(id);

                if (provider == null)
                {
                    return NotFound("Provider not found.");
                }

                _logger.LogInformation("Admin retrieved provider details for ID: {ProviderId}", id);
                return Ok(provider);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid provider ID format: {ProviderId}", id);
                return BadRequest("Invalid provider ID format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving provider {ProviderId}", id);
                return StatusCode(500, "An error occurred while retrieving provider details.");
            }
        }

        /// <summary>
        /// Retrieves a specific provider by tenant ID (Admin only).
        /// </summary>
        /// <param name="tenantId">Tenant unique identifier</param>
        /// <returns>Provider details.</returns>
        /// <response code="200">Returns the provider details successfully.</response>
        /// <response code="400">Invalid tenant ID format.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Provider not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("providers/tenant/{tenantId:guid}")]
        [SwaggerOperation(
            Summary = "Get provider by tenant ID",
            Description = "Retrieves detailed information about a provider by their tenant identifier."
        )]
        [ProducesResponseType(typeof(ProviderManagementDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProviderManagementDto>> GetProviderByTenantId(Guid tenantId)
        {
            try
            {
                if (tenantId == Guid.Empty)
                {
                    return BadRequest("Invalid tenant ID provided.");
                }

                var provider = await _providerManagementService.GetProviderByTenantIdAsync(tenantId);

                if (provider == null)
                {
                    return NotFound("Provider not found.");
                }

                _logger.LogInformation("Admin retrieved provider details for tenant ID: {TenantId}", tenantId);
                return Ok(provider);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tenant ID format: {TenantId}", tenantId);
                return BadRequest("Invalid tenant ID format.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving provider by tenant ID {TenantId}", tenantId);
                return StatusCode(500, "An error occurred while retrieving provider details.");
            }
        }

        /// <summary>
        /// Updates provider verification status (Admin only).
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="request">Verification update request</param>
        /// <returns>Success status of the verification update.</returns>
        /// <response code="200">Verification status updated successfully.</response>
        /// <response code="400">Invalid request data or tenant ID.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Provider not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPut("providers/{tenantId:guid}/verification")]
        [SwaggerOperation(
            Summary = "Update provider verification status",
            Description = "Updates the email and phone verification status for a specific provider."
        )]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> UpdateProviderVerification(
            Guid tenantId,
            [FromBody] UpdateVerificationRequestDto request)
        {
            try
            {
                if (tenantId == Guid.Empty)
                {
                    return BadRequest("Invalid tenant ID provided.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerManagementService.UpdateProviderVerificationAsync(
                    tenantId,
                    request.IsEmailVerified,
                    request.IsPhoneVerified);

                if (!result)
                {
                    return NotFound("Provider not found or verification update failed.");
                }

                _logger.LogInformation("Admin updated verification status for provider {TenantId}: Email={EmailVerified}, Phone={PhoneVerified}",
                    tenantId, request.IsEmailVerified, request.IsPhoneVerified);

                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for verification update: {TenantId}", tenantId);
                return BadRequest("Invalid request data provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating provider verification {TenantId}", tenantId);
                return StatusCode(500, "An error occurred while updating provider verification.");
            }
        }

        /// <summary>
        /// Locks or unlocks a provider account (Admin only).
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="request">Account lock request</param>
        /// <returns>Success status of the account lock operation.</returns>
        /// <response code="200">Account lock status updated successfully.</response>
        /// <response code="400">Invalid request data or tenant ID.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="404">Provider not found.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpPut("providers/{tenantId:guid}/lock")]
        [SwaggerOperation(
            Summary = "Lock or unlock provider account",
            Description = "Locks a provider account until a specified date or unlocks it immediately."
        )]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<bool>> LockProviderAccount(
            Guid tenantId,
            [FromBody] LockAccountRequestDto request)
        {
            try
            {
                if (tenantId == Guid.Empty)
                {
                    return BadRequest("Invalid tenant ID provided.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _providerManagementService.LockProviderAccountAsync(
                    tenantId,
                    request.LockUntil);

                if (!result)
                {
                    return NotFound("Provider not found or account lock operation failed.");
                }

                var action = request.LockUntil.HasValue ? "locked" : "unlocked";
                _logger.LogInformation("Admin {Action} provider account {TenantId} until {LockUntil}",
                    action, tenantId, request.LockUntil);

                return Ok(true);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for account lock: {TenantId}", tenantId);
                return BadRequest("Invalid request data provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while locking provider account {TenantId}", tenantId);
                return StatusCode(500, "An error occurred while updating account lock status.");
            }
        }

        /// <summary>
        /// Retrieves provider statistics for dashboard (Admin only).
        /// </summary>
        /// <returns>Provider statistics including counts and trends.</returns>
        /// <response code="200">Returns provider statistics successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="403">User does not have admin privileges.</response>
        /// <response code="500">Internal server error occurred.</response>
        [HttpGet("statistics")]
        [SwaggerOperation(
            Summary = "Get provider statistics",
            Description = "Retrieves comprehensive statistics about providers for admin dashboard."
        )]
        [ProducesResponseType(typeof(ProviderStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProviderStatisticsDto>> GetProviderStatistics()
        {
            try
            {
                var statistics = await _providerManagementService.GetProviderStatisticsAsync();

                _logger.LogInformation("Admin retrieved provider statistics: Total={Total}, Active={Active}",
                    statistics.TotalProviders, statistics.ActiveProviders);

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving provider statistics");
                return StatusCode(500, "An error occurred while retrieving provider statistics.");
            }
        }
    }
}
