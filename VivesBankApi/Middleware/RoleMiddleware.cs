using System.Security.Claims;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;

namespace VivesBankApi.Middleware;

public class RoleMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly IUserService _userService;

    public RoleMiddleware(ILogger<RoleMiddleware> logger, RequestDelegate next, IUserService userService)
    {
        _logger = logger;
        _next = next;
        _userService = userService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            _logger.LogDebug("User is authenticated");
            var userId = context.User.FindFirst("UserId")?.Value;
            var user = await _userService.GetUserByIdAsync(userId);
            if (user != null)
            {
                _logger.LogDebug("User found adding roles to claims");
                var claims = new List<Claim>
                {
                    new("UserId", userId),
                    new(ClaimTypes.Role, typeof(Role).GetEnumName(user.Role))
                };
                var identity = new ClaimsIdentity(claims, "custom");
                _logger.LogDebug("Adding identity to user");
                context.User.AddIdentity(identity);
            }
        }
        _logger.LogInformation("Invoking next middleware");
        await _next(context);
    }
    
}