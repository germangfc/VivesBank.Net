using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Middleware.Jwt;

/// <summary>
/// Interfaz para la generación de tokens JWT.
/// Define un método para generar un token JWT para un usuario específico.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
/// <version>1.0</version>
public interface IJwtGenerator
{
    /// <summary>
    /// Genera un token JWT para el usuario proporcionado.
    /// </summary>
    /// <param name="user">El usuario para el que se generará el token JWT.</param>
    /// <returns>Un token JWT válido para el usuario.</returns>
    string GenerateJwtToken(User user);
}