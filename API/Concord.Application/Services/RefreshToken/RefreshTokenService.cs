using Concord.Domain.Context.Identity;
using System.Security.Cryptography;

namespace Concord.Application.Services.RefreshToken
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IdentityDbContext _context;
        public RefreshTokenService(IdentityDbContext context)
        {
            _context = context;
        }
        public async Task<string> RefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var generator = new RNGCryptoServiceProvider();
            generator.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);
            var tokenInUser = _context.Users.Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                randomNumber = new byte[64];
                generator.GetBytes(randomNumber);
                refreshToken = Convert.ToBase64String(randomNumber);
                tokenInUser = _context.Users.Any(a => a.RefreshToken == refreshToken);
            }
            return refreshToken;
        }

    }
}
