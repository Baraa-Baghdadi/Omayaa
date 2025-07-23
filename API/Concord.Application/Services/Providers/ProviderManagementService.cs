using Concord.Application.DTO.Provider;
using Concord.Domain.Context.Identity;
using Concord.Domain.Models.Identity;
using Concord.Domain.Models.Providers;
using Concord.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Concord.Application.Services.Providers
{
    /// <summary>
    /// Service implementation for provider management operations (Admin only)
    /// </summary>
    public class ProviderManagementService : IProviderManagementService
    {
        private readonly IGenericRepository<Provider> _providerRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdentityDbContext _identityContext;
        public ProviderManagementService(
           IGenericRepository<Provider> providerRepository,
           UserManager<ApplicationUser> userManager,
           IdentityDbContext identityContext)
        {
            _providerRepository = providerRepository;
            _userManager = userManager;
            _identityContext = identityContext;
        }

        /// <summary>
        /// Gets all providers with advanced filtering, sorting and pagination
        /// </summary>
        /// <param name="request">Request parameters for filtering and pagination</param>
        /// <returns>Paginated list of providers with detailed information</returns>
        public async Task<GetProvidersResponseDto> GetAllProvidersAsync(GetProvidersRequestDto request)
        {
            try
            {
                // Build filter expression
                Expression<Func<Provider, bool>>? filter = BuildFilterExpression(request);

                // Get total count for pagination
                var totalCount = await _providerRepository.CountAsync(filter);

                // Calculate pagination values
                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
                var hasNextPage = request.PageNumber < totalPages;
                var hasPreviousPage = request.PageNumber > 1;
                var startItem = (request.PageNumber - 1) * request.PageSize + 1;
                var endItem = Math.Min(request.PageNumber * request.PageSize, totalCount);

                // Build order expression
                Func<IQueryable<Provider>, IOrderedQueryable<Provider>> orderBy = BuildOrderExpression(request);

                // Get providers with pagination
                var providers = await _providerRepository.GetAllWithFilterAsync(
                    filter: filter,
                    orderBy: orderBy,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize
                );

                // Get associated user information for each provider
                var providerDtos = new List<ProviderManagementDto>();

                foreach (var provider in providers)
                {
                    // Find associated user by TenantId
                    var user = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.TenantId == provider.TenantId);

                    var providerDto = new ProviderManagementDto
                    {
                        Id = provider.Id,
                        TenantId = provider.TenantId,
                        ProviderName = provider.ProviderName,
                        Telephone = provider.Telephone,
                        Mobile = provider.Mobile,
                        Address = provider.Address,
                        CreationTime = provider.CreationTime,
                        Email = user?.Email,
                        DisplayName = user?.DisplayName,
                        IsEmailVerified = user?.EmailConfirmed ?? false,
                        IsPhoneVerified = user?.PhoneNumberConfirmed ?? false,
                        AccountStatus = GetAccountStatus(user),
                        LastLoginDate = null, // You can implement last login tracking if needed
                        LockoutEnd = user?.LockoutEnd,
                        LockoutEnabled = user?.LockoutEnabled ?? false,
                        AccessFailedCount = user?.AccessFailedCount ?? 0
                    };

                    providerDtos.Add(providerDto);
                }

                // Apply additional filters that require user data
                if (!string.IsNullOrWhiteSpace(request.AccountStatus))
                {
                    providerDtos = providerDtos
                        .Where(p => p.AccountStatus.Equals(request.AccountStatus, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                if (request.IsVerified.HasValue)
                {
                    providerDtos = providerDtos
                        .Where(p => (p.IsEmailVerified && p.IsPhoneVerified) == request.IsVerified.Value)
                        .ToList();
                }

                return new GetProvidersResponseDto
                {
                    Providers = providerDtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = request.PageNumber,
                        TotalPages = totalPages,
                        PageSize = request.PageSize,
                        TotalCount = totalCount,
                        SearchTerm = request.SearchTerm,
                        HasNextPage = hasNextPage,
                        HasPreviousPage = hasPreviousPage,
                        StartItem = startItem,
                        EndItem = endItem
                    }
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving providers: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific provider by ID
        /// </summary>
        /// <param name="providerId">Provider unique identifier</param>
        /// <returns>Provider details</returns>
        public async Task<ProviderManagementDto?> GetProviderByIdAsync(Guid providerId)
        {
            try
            {
                var provider = await _providerRepository.GetByIdAsync(providerId);
                if (provider == null) return null;

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.TenantId == provider.TenantId);

                return new ProviderManagementDto
                {
                    Id = provider.Id,
                    TenantId = provider.TenantId,
                    ProviderName = provider.ProviderName,
                    Telephone = provider.Telephone,
                    Mobile = provider.Mobile,
                    CreationTime = provider.CreationTime,
                    Email = user?.Email,
                    DisplayName = user?.DisplayName,
                    IsEmailVerified = user?.EmailConfirmed ?? false,
                    IsPhoneVerified = user?.PhoneNumberConfirmed ?? false,
                    AccountStatus = GetAccountStatus(user),
                    LastLoginDate = null,
                    LockoutEnd = user?.LockoutEnd,
                    LockoutEnabled = user?.LockoutEnabled ?? false,
                    AccessFailedCount = user?.AccessFailedCount ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving provider: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets a specific provider by tenant ID
        /// </summary>
        /// <param name="tenantId">Tenant unique identifier</param>
        /// <returns>Provider details</returns>
        public async Task<ProviderManagementDto?> GetProviderByTenantIdAsync(Guid tenantId)
        {
            try
            {
                var provider = await _providerRepository.GetFirstOrDefault(p => p.TenantId == tenantId);
                if (provider == null) return null;

                return await GetProviderByIdAsync(provider.Id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving provider by tenant ID: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates provider verification status
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="isEmailVerified">Email verification status</param>
        /// <param name="isPhoneVerified">Phone verification status</param>
        /// <returns>Success status</returns>
        public async Task<bool> UpdateProviderVerificationAsync(Guid tenantId, bool isEmailVerified, bool isPhoneVerified)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
                if (user == null)
                    throw new Exception("Provider not found");

                user.EmailConfirmed = isEmailVerified;
                user.PhoneNumberConfirmed = isPhoneVerified;

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating provider verification: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Locks or unlocks a provider account
        /// </summary>
        /// <param name="tenantId">Tenant ID of the provider</param>
        /// <param name="lockUntil">Lock until date (null to unlock)</param>
        /// <returns>Success status</returns>
        public async Task<bool> LockProviderAccountAsync(Guid tenantId, DateTimeOffset? lockUntil)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
                if (user == null)
                    throw new Exception("Provider not found");

                var result = await _userManager.SetLockoutEndDateAsync(user, lockUntil);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error locking provider account: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets provider statistics for dashboard
        /// </summary>
        /// <returns>Provider statistics</returns>
        public async Task<ProviderStatisticsDto> GetProviderStatisticsAsync()
        {
            try
            {
                var totalProviders = await _providerRepository.CountAsync();

                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                var registeredToday = await _providerRepository.CountAsync(p => p.CreationTime.Date == today);
                var registeredThisMonth = await _providerRepository.CountAsync(p => p.CreationTime >= startOfMonth);

                // Get user statistics
                var allUsers = await _userManager.Users.Where(u => u.Role == "Provider").ToListAsync();

                var activeProviders = allUsers.Count(u => GetAccountStatus(u) == "Active");
                var inactiveProviders = allUsers.Count(u => GetAccountStatus(u) == "Inactive");
                var suspendedProviders = allUsers.Count(u => GetAccountStatus(u) == "Suspended");
                var pendingProviders = allUsers.Count(u => GetAccountStatus(u) == "Pending Verification");

                return new ProviderStatisticsDto
                {
                    TotalProviders = totalProviders,
                    ActiveProviders = activeProviders,
                    InactiveProviders = inactiveProviders,
                    SuspendedProviders = suspendedProviders,
                    PendingVerificationProviders = pendingProviders,
                    RegisteredThisMonth = registeredThisMonth,
                    RegisteredToday = registeredToday
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving provider statistics: {ex.Message}", ex);
            }
        }

        #region Private Methods

        /// <summary>
        /// Builds filter expression based on request parameters
        /// </summary>
        private Expression<Func<Provider, bool>>? BuildFilterExpression(GetProvidersRequestDto request)
        {
            Expression<Func<Provider, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTermLower = request.SearchTerm.Trim().ToLower();
                filter = p => p.ProviderName.ToLower().Contains(searchTermLower) ||
                             p.Mobile.Contains(searchTermLower) ||
                             p.Telephone.Contains(searchTermLower);
            }

            return filter;
        }

        /// <summary>
        /// Builds order expression based on request parameters
        /// </summary>
        private Func<IQueryable<Provider>, IOrderedQueryable<Provider>> BuildOrderExpression(GetProvidersRequestDto request)
        {
            return request.SortBy?.ToLower() switch
            {
                "providername" => request.SortDirection?.ToLower() == "asc"
                    ? q => q.OrderBy(p => p.ProviderName)
                    : q => q.OrderByDescending(p => p.ProviderName),
                "mobile" => request.SortDirection?.ToLower() == "asc"
                    ? q => q.OrderBy(p => p.Mobile)
                    : q => q.OrderByDescending(p => p.Mobile),
                _ => request.SortDirection?.ToLower() == "asc"
                    ? q => q.OrderBy(p => p.CreationTime)
                    : q => q.OrderByDescending(p => p.CreationTime)
            };
        }

        /// <summary>
        /// Helper method to determine account status based on user properties
        /// </summary>
        private string GetAccountStatus(ApplicationUser? user)
        {
            if (user == null)
                return "غير نشط";

            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
                return "معطل";

            if (!user.EmailConfirmed || !user.PhoneNumberConfirmed)
                return "بانتظار التنشيط";

            return "نشط";
        }

        #endregion

    }
}
