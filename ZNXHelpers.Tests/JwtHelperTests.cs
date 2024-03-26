using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace ZNXHelpers.Tests;

public class JwtHelperTests
{
    private readonly JwtHelper _jwtHelper;

    public JwtHelperTests()
    {
        _jwtHelper = new JwtHelper();
    }

    private readonly string _testKey = "12341234123412341234123412341234";

    public string GenerateTestToken()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes(_testKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] 
            {
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.Role, "Test Role"),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public void TestValidateJwt()
    {
        // Arrange
        var token = GenerateTestToken();
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(_testKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
        List<Claim> claims;
        SecurityToken securityToken;
        List<Claim> claims2;
        SecurityToken securityToken2;

        // Act
        var result = _jwtHelper.ValidateJwt(token, tokenValidationParameters, out claims, out securityToken);

        // Assert
        Assert.True(result);
        Assert.NotNull(claims);
        Assert.NotNull(securityToken);
        
        // Act
        var result2 = _jwtHelper.ValidateJwt("not a token", tokenValidationParameters, out claims2, out securityToken2);

        // Assert
        Assert.False(result2);
        Assert.Null(claims2);
        Assert.Null(securityToken2);
    }

    [Fact]
    public void TestCreateJws()
    {
        // Arrange
        var issuer = "test";
        var audience = "testaud";
        var claims = new List<Claim>();
        var notBefore = DateTime.UtcNow;
        var expires = DateTime.UtcNow.AddHours(1);
        var signingCertificate = new X509Certificate2("test.pfx", "chef$123");
        var signingCredentials = new X509SigningCredentials(signingCertificate, "RS256");

        // Act
        var result = _jwtHelper.CreateJws(issuer, audience, claims, notBefore, expires, signingCredentials);

        // Assert
        Assert.NotNull(result);
        
        // Act
        var result2 = _jwtHelper.CreateJws(issuer, audience, claims, notBefore, expires, signingCertificate);

        // Assert
        Assert.NotNull(result2);
    }

    // Add more tests for other methods in the JwtHelper class
}