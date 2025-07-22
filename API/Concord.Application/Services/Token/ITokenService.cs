using Concord.Domain.Models.Identity;

namespace Concord.Application.Services.Token
{
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user);
    }
}
