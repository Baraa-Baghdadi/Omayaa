using System.ComponentModel.DataAnnotations;

namespace Concord.Application.DTO.Provider
{
    public class GetProvidersRequestDto
    {
        /// <summary>
        /// Page number (starting from 1)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (1-100)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Search term to filter providers by name, mobile, or telephone
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filter by account status
        /// </summary>
        public string? AccountStatus { get; set; }

        /// <summary>
        /// Filter by verification status
        /// </summary>
        public bool? IsVerified { get; set; }

        /// <summary>
        /// Sort field (CreationTime, ProviderName, Mobile)
        /// </summary>
        public string SortBy { get; set; } = "CreationTime";

        /// <summary>
        /// Sort direction (asc, desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }
}
