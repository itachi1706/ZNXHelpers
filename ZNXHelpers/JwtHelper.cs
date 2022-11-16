using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace ZNXHelpers
{
    public class JwtHelper
    {
        public bool ValidateJwt(string token, TokenValidationParameters tokenValidationParameters, out List<Claim>? claims, out SecurityToken? securityToken)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                claims = new List<Claim>(claimsPrincipal.Claims);
                return true;
            } catch
            {
                claims = null;
                securityToken = null;
                return false;
            }
        }

        public string CreateJws(
            string? issuer = null,
            string? audience = null,
            IEnumerable<Claim>? claims = null,
            DateTime? notBefore = null,
            DateTime? expires = null,
            X509Certificate2? signingCertificate = null)
        {
            X509SigningCredentials signingCredentials = new X509SigningCredentials(signingCertificate, "RS256");

            return CreateJws(issuer, audience, claims, notBefore, expires, signingCredentials);
        }

        public string CreateJws(
            string? issuer = null,
            string? audience = null,
            IEnumerable<Claim>? claims = null,
            DateTime? notBefore = null,
            DateTime? expires = null,
            SigningCredentials? signingCredentials = null)
        {
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(issuer, audience, claims, notBefore, expires, signingCredentials);
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            string token = handler.WriteToken(jwtSecurityToken);
            return token;
        }
    }
}
