using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Middleware.Jwt
{
    /// <summary>
    /// Clase para la generación de tokens JWT.
    /// Implementa la interfaz IJwtGenerator para crear tokens JWT para usuarios.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class JwtGenerator : IJwtGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<JwtGenerator> _logger;
        private readonly AuthJwtConfig _authConfig;

        /// <summary>
        /// Constructor que inicializa el generador de tokens JWT con las dependencias necesarias.
        /// </summary>
        public JwtGenerator(IHttpContextAccessor httpContextAccessor, ILogger<JwtGenerator> logger,
            AuthJwtConfig authConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _authConfig = authConfig;
        }

        /// <summary>
        /// Genera un token JWT para el usuario proporcionado.
        /// </summary>
        /// <param name="user">El usuario para el cual se generará el token JWT.</param>
        /// <returns>El token JWT generado como una cadena.</returns>
        public string GenerateJwtToken(User user)
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
}