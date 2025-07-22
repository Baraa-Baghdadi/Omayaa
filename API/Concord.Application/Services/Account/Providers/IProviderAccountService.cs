using Concord.Application.DTO.Identity;
using System.Security.Claims;

namespace Concord.Application.Services.Account.Providers
{
    public interface IProviderAccountService
    {
        Task<UserInfo> GetCurrentUser(ClaimsPrincipal currentUser);
        Task<UserDto> Login(LoginDto loginDto);
        Task<bool> Regitser(ProviderRegisterDto registerDto);
        Task<TokenApiDto> Refresh(TokenApiDto tokenApiDto);
        Task<bool> ChangePassword(ChangePasswordDto input);
        Task<bool> ForgotPassword(string email);
        Task<bool> ResetPassword(ResetPasswordDto model);
        Task<bool> VerifyAccount(Guid tenantId);

    }
}
