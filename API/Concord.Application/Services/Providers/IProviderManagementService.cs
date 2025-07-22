using Concord.Application.DTO.Provider;

namespace Concord.Application.Services.Providers
{
    /// <summary>
    /// Service interface for provider management operations (Admin only)
    /// </summary>
    public interface IProviderManagementService
    {
        /// <summary>
        /// Gets all providers with advanced filtering, sorting and pagination
        /// </summary>
        /// <param name="request">Request parameters for filtering and pagination</param>
        /// <returns>Paginated list of providers with detailed information</returns>
        Task<GetProvidersResponseDto> GetAllProvidersAsync(GetProvidersRequestDto request);

        /// <summary>
        /// Gets a specific provider by ID
        /// </summary>
        /// <param name="providerId">Provider unique identifier</param>
        /// <returns>Provider details</returns>
        Task<ProviderManagementDto?> GetProviderByIdAsync(Guid providerId);

        /// <summary>
        /// Gets a specific provider by tenant ID
        /// </summary>
        /// <param name="tenantId">Tenant unique identifier</param>
        /// <returns>Provider details</returns>
        Task<ProviderManagementDto?> GetProviderByTenantIdAsync(Guid tenantId);

        /// <summary>
        /// Updates provider verification status
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="isEmailVerified">Email verification status</param>
        /// <param name="isPhoneVerified">Phone verification status</param>
        /// <returns>Success status</returns>
        Task<bool> UpdateProviderVerificationAsync(Guid tenantId, bool isEmailVerified, bool isPhoneVerified);

        /// <summary>
        /// Locks or unlocks a provider account
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="lockUntil">Lock until date (null to unlock)</param>
        /// <returns>Success status</returns>
        Task<bool> LockProviderAccountAsync(Guid tenantId, DateTimeOffset? lockUntil);

        /// <summary>
        /// Gets provider statistics for dashboard
        /// </summary>
        /// <returns>Provider statistics</returns>
        Task<ProviderStatisticsDto> GetProviderStatisticsAsync();
    }
}
