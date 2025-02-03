using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

/// <summary>
/// Representa la solicitud para iniciar sesión con DNI y contraseña.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Número de documento de identidad (DNI) del usuario.
    /// Debe ser exactamente de 9 caracteres.
    /// </summary>
    [Required]
    [StringLength(9, ErrorMessage = "Must be a DNI")]
    [JsonProperty("dni")]
    public string Dni { get; set; }

    /// <summary>
    /// Contraseña del usuario.
    /// Debe tener entre 8 y 50 caracteres.
    /// </summary>
    [Required]
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")]
    public string Password { get; set; }
}
