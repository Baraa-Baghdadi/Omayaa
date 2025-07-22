using System.Security.Claims;

namespace Concord.Application.Extentions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string RetrieveEmailFromPrincipal(this ClaimsPrincipal user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.FindFirstValue(ClaimTypes.Email);
        }

    }
}
