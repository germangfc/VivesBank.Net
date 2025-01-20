using System.Security.Claims;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;

namespace VivesBankApi.Middleware;

public class RoleMiddleware
{
    private readonly ILogger _logger;
    private readonly RequestDelegate _next;
    private readonly IServiceProvider _serviceProvider;

    public RoleMiddleware(ILogger<RoleMiddleware> logger, RequestDelegate next, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var _userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("UserId")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _userService.GetUserByIdAsync(userId);

                    if (user != null)
                    {
                        _logger.LogDebug("User found. Adding roles to claims.");

                        var claims = new List<Claim>
                        {
                            new("UserId", userId),
                            new(ClaimTypes.Role, typeof(Role).GetEnumName(user.Role) ?? string.Empty)
                        };

                        _logger.LogDebug("Adding identity to user.");
                        var identity = new ClaimsIdentity(claims, "custom");
                        context.User.AddIdentity(identity);
                    }
                    else
                    {
                        _logger.LogWarning("No user found for UserId: {UserId}", userId);
                    }
                }
                else
                {
                    _logger.LogWarning("UserId claim is missing for authenticated user.");
                }
            }
            else
            {
                _logger.LogInformation("Request is not authenticated: {Path}", context.Request.Path);
            }
        }

        _logger.LogInformation("Invoking next middleware.");
        await _next(context);
    }
    
}