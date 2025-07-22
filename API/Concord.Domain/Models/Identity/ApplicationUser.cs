using Microsoft.AspNetCore.Identity;
namespace Concord.Domain.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public Guid? TenantId { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public string? AccessToken { get; set; }

        // For Refeesh token:
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiredTime { get; set; }

    }
}
