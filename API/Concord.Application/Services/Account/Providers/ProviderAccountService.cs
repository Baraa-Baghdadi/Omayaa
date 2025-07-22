using Concord.Application.DTO.Identity;
using Concord.Application.Extentions;
using Concord.Application.Services.Mail;
using Concord.Application.Services.RefreshToken;
using Concord.Application.Services.Token;
using Concord.Domain.Context.Identity;
using Concord.Domain.Models.Identity;
using Concord.Domain.Models.Providers;
using Concord.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Concord.Application.Services.Account.Providers
{
    public class ProviderAccountService : IProviderAccountService
    {
        private readonly IGenericRepository<Provider> _providerRepository;
        private readonly IdentityDbContext _identityContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IGetPrincipleDataFromExpiredToken _getPrincipleFromExToken;
        private readonly IMailingService _mailingService;

        public ProviderAccountService(IGenericRepository<Provider> providerRepository, 
            IdentityDbContext identityContext, UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, ITokenService tokenService, 
            IRefreshTokenService refreshTokenService, 
            IGetPrincipleDataFromExpiredToken getPrincipleFromExToken, 
            IMailingService mailingService)
        {
            _providerRepository = providerRepository;
            _identityContext = identityContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _getPrincipleFromExToken = getPrincipleFromExToken;
            _mailingService = mailingService;
        }

        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            var user = await _userManager.FindByEmailAsync(input.Email);
            if (user == null) throw new Exception("هذا المستخدم غير موجود");

            if (input.NewPassword != input.ConfirmPassword)
            {
                throw new Exception("كلمة المرور غير صحيحة");
            }

            var result = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
            if (!result.Succeeded) throw new Exception("كلمة المرور غير جيدة");
            return true;

        }

        public async Task<bool> ForgotPassword(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Handle case where email is not found
                throw new Exception("المستخدم غير موجود");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            // Send the token to the user via email or other means
            // Handle sending the email here or in a separate service
            var filePath = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Templates\\EmailTemplate.HTML";
            var str = new StreamReader(filePath);
            var mailText = str.ReadToEnd();
            str.Close();
            mailText = mailText.Replace("[username]", user.DisplayName).Replace("[email]", user.Email).Replace("[token]", token);
            await _mailingService.SendEmailAsync(user.Email, "Reset Password", mailText);
            return true;

        }

        public async Task<UserInfo> GetCurrentUser(ClaimsPrincipal currentUser)
        {
            var user = await _userManager.FindByEmailFromClaimsPrincipal(currentUser);

            if (user is null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return new UserInfo
            {
                Email = user.Email,
                DisplayName = user.DisplayName,
                TenantId = user.TenantId,
                Role = user.Role
            };

        }

        public async Task<UserDto> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.DisplayName == loginDto.ProviderName)
                ?? await _userManager.FindByNameAsync(loginDto.ProviderName);
            if (user == null) throw new Exception("هذا المستخدم غير موجود");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) throw new Exception("كلمة مرور غير صحيحة");

            user.AccessToken = _tokenService.CreateToken(user);
            user.RefreshToken = await _refreshTokenService.RefreshTokenAsync();
            user.RefreshTokenExpiredTime = DateTime.Now.AddDays(10);

            await _identityContext.SaveChangesAsync();

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,
                AccessToken = _tokenService.CreateToken(user),
                RefreshToken = user.RefreshToken,
                RefreshTokenExpiredTime = user.RefreshTokenExpiredTime
            };

        }

        public async Task<TokenApiDto> Refresh(TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
            {
                throw new Exception("Invalid Client Request");
            }
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principle = _getPrincipleFromExToken.GetPrincipleData(accessToken);
            var name = principle.Identity.Name;
            var user = await _userManager.FindByNameAsync(name);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiredTime <= DateTime.Now)
            {
                throw new Exception("Invalid Request,You Should Login again!!");
            }
            var newAccessToken = _tokenService.CreateToken(user);
            var newRefreshToken = await _refreshTokenService.RefreshTokenAsync();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiredTime = DateTime.Now.AddDays(5);
            await _identityContext.SaveChangesAsync();
            return new TokenApiDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken };

        }

        public async Task<bool> Regitser(ProviderRegisterDto registerDto)
        {
            var random = new Random();
            var isEmailExist = await CheckEmailExistsAsync(registerDto.Mobile);
            if (isEmailExist) throw new Exception("إن رقم الموبايل هذا مستخدم مسبقا");

            if (registerDto.Password != registerDto.ConfirmPassword)
                throw new Exception("كلمة المرور والتأكيد يجب أن تكون متطابقة");

            var tenantId = Guid.NewGuid();

            var user = new ApplicationUser
            {
                TenantId = tenantId,
                DisplayName = registerDto.ProviderName,
                PhoneNumber = registerDto.Mobile,
                Email = tenantId.ToString() + "@omayya.com",
                UserName = tenantId.ToString(),
                Role = "Provider"
            };

            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            await _userManager.AddToRoleAsync(user, "Provider");


            var provider = new Provider(Guid.NewGuid(),tenantId,registerDto.ProviderName,registerDto.Telephone,registerDto.Mobile,DateTime.Now);

            await _providerRepository.AddAsync(provider);
            await _providerRepository.SaveChangesAsync();

            return true;

        }

        public async Task<bool> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                throw new Exception("المستخدم غير موجود");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded) return true;
            else throw new Exception(result.Errors.ToString());

        }

        public async Task<bool> VerifyAccount(Guid tenantId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId);
            user.PhoneNumberConfirmed = true;
            user.EmailConfirmed = true;
            return true;
        }

        #region private_Methods
        private async Task<bool> CheckEmailExistsAsync(string mobile)
        { 
            return _userManager.Users.Any(x => x.PhoneNumber!.Trim() == mobile);
        }

        #endregion

    }
}
