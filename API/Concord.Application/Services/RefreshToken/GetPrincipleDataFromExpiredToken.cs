using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Concord.Application.Services.RefreshToken
{
    public class GetPrincipleDataFromExpiredToken : IGetPrincipleDataFromExpiredToken
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public GetPrincipleDataFromExpiredToken(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:key"]));
        }

        public ClaimsPrincipal GetPrincipleData(string token)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            var tokenValidationParameter = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:key"])),
                ValidIssuer = _config["Token:Issuer"],
                ValidateIssuer = true,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principle = tokenHandler.ValidateToken(token, tokenValidationParameter, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            // jwtSecurityToken contain principle as email and username
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException($"This Token Is Invalid");
            }
            return principle;
        }

    }
}
