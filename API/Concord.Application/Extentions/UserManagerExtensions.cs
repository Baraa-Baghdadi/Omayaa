using Concord.Domain.Models.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Concord.Application.Extentions
{
    public static class UserManagerExtensions
    {
        public static async Task<ApplicationUser> FindUserByClaimsPrincipleWithAddress(this UserManager<ApplicationUser> userManager,
            ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Email);

            return await userManager.Users
                .SingleOrDefaultAsync(x => x.Email == email);
        }

        public static async Task<ApplicationUser> FindByEmailFromClaimsPrincipal(this UserManager<ApplicationUser> userManager,
           ClaimsPrincipal user)
        {
            return await userManager.Users.SingleOrDefaultAsync(x => x.Email == user.FindFirstValue(ClaimTypes.Email));
        }

    }
}
