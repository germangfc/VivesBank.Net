namespace VivesBankApi.Database;

/// <summary>
/// Configuración para el manejo de tokens JWT utilizados en la autenticación.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
/// <version>1.0</version>
public class AuthJwtConfig
{
    /// <summary>
    /// Clave secreta utilizada para firmar el token JWT.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Emisor del token JWT.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Audiencia para la que está destinado el token JWT.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Tiempo de expiración del token JWT en minutos.
    /// </summary>
    public string ExpiresInMinutes { get; set; } = string.Empty;
}