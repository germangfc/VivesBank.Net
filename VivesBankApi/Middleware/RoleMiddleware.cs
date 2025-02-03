using System.Security.Claims;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;

namespace VivesBankApi.Middleware;

/// <summary>
    /// Middleware que maneja el rol del usuario autenticado y lo agrega a las reclamaciones.
    /// Valida si el rol del usuario es correcto y lo asigna al contexto de la solicitud.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class RoleMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor que recibe el logger, el siguiente middleware y el proveedor de servicios.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos.</param>
        /// <param name="next">El siguiente middleware en la cadena.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public RoleMiddleware(ILogger<RoleMiddleware> logger, RequestDelegate next, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _next = next;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Método que procesa la solicitud, validando el rol del usuario y añadiéndolo al contexto si es válido.
        /// </summary>
        /// <param name="context">El contexto HTTP de la solicitud.</param>
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
                            if (!Enum.IsDefined(typeof(Role), user.Role))
                            {
                                _logger.LogError("Invalid role value received: {Role}", user.Role);
                            }
                            else
                            {
                                if (Enum.TryParse(typeof(Role), user.Role?.ToString(), out var roleEnum) && Enum.IsDefined(typeof(Role), roleEnum))
                                {
                                    var roleName = Enum.GetName(typeof(Role), roleEnum) ?? string.Empty;
                                    var claims = new List<Claim>
                                    {
                                        new("UserId", userId),
                                        new(ClaimTypes.Role, roleName),
                                        new (ClaimTypes.NameIdentifier, userId),
                                    };

                                    if (context.User.Identities.Any())
                                    {
                                        context.User = new ClaimsPrincipal();
                                    }
                                    _logger.LogDebug($"Role {roleName}");
                                    _logger.LogDebug("Adding identity to user.");
                                    var identity = new ClaimsIdentity(claims, "custom");
                                    context.User.AddIdentity(identity);
                                }
                                else
                                {
                                    _logger.LogError("Invalid role value received: {Role}", user.Role);
                                }
                            }
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