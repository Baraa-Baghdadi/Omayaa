using System.Security.Claims;

namespace Concord.Application.Services.RefreshToken
{
    public interface IGetPrincipleDataFromExpiredToken
    {
        ClaimsPrincipal GetPrincipleData(string token);
    }
}
