using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ZNXHelpers
{
    public class JwtHelper
    {
        public bool ValidateJwt(string token, TokenValidationParameters tokenValidationParameters, out List<Claim> claims, out SecurityToken securityToken)
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
    }
}
