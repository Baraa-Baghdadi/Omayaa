using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concord.Application.DTO.Provider
{
    public class ProviderManagementDto
    {
        /// <summary>
        /// Provider unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tenant unique identifier
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Provider name
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Provider telephone number
        /// </summary>
        public string Telephone { get; set; } = string.Empty;

        /// <summary>
        /// Provider mobile number
        /// </summary>
        public string Mobile { get; set; } = string.Empty;

        /// <summary>
        /// Account creation date and time
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Associated user email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// User display name
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Account status (Active, Inactive, Suspended, Pending)
        /// </summary>
        public string AccountStatus { get; set; } = "Active";

        /// <summary>
        /// Whether the email is verified
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// Whether the phone number is verified
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// Last login date
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Account lockout end date (if locked)
        /// </summary>
        public DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Whether lockout is enabled for this account
        /// </summary>
        public bool LockoutEnabled { get; set; }

        /// <summary>
        /// Number of failed login attempts
        /// </summary>
        public int AccessFailedCount { get; set; }
    }
}
