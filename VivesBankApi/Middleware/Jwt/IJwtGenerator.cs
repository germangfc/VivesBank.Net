using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Middleware.Jwt;

public interface IJwtGenerator
{
    String GenerateJwtToken(User user);
}