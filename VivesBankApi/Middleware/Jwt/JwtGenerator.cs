using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Middleware.Jwt;

public class JwtGenerator: IJwtGenerator
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<JwtGenerator> _logger;
    private readonly AuthJwtConfig _authConfig;

    public JwtGenerator(IHttpContextAccessor httpContextAccessor, ILogger<JwtGenerator> logger,
        AuthJwtConfig authConfig)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _authConfig = authConfig;
    }
    
    public String GenerateJwtToken(User user)
    {
        _logger.LogInformation("Generating JWT token");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _logger.LogInformation($"Inserting id to Claims: {user.Id}");
        var claims = new[]
        {
            new Claim("UserId", user.Id)
        };
        var token = new JwtSecurityToken(
            _authConfig.Issuer,
            _authConfig.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_authConfig.ExpiresInMinutes)),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}