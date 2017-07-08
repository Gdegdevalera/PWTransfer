using System;
using Shared;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace AuthService.Service
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly string _securityKey;

        public JwtGenerator(string securityKey)
        {
            _securityKey = securityKey;
        }

        public string GenerateJwt(Data.User user)
        {
            var now = DateTime.UtcNow;
            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_securityKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = GetIdentity(user),
                NotBefore = now,
                Expires = now.Add(TimeSpan.FromHours(1)),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256),
            };

            var encodedJwt = new JwtSecurityTokenHandler().CreateEncodedJwt(tokenDescriptor);

            return encodedJwt;
        }

        private ClaimsIdentity GetIdentity(Data.User user)
        {
            var claims = new Claim[]
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.Name),
                    new Claim("userId", user.Id.ToString()),
                    new Claim("email", user.Email)
                };

            var claimsIdentity = new ClaimsIdentity(claims, 
                "Token", 
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }
    }
}
