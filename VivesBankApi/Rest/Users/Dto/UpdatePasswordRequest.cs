using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class UpdatePasswordRequest
{
    /// <summary>
    /// Nueva contraseña del usuario.
    /// Debe tener entre 8 y 50 caracteres.
    /// </summary>
    [MinLength(8, ErrorMessage = "The password must be at least 8 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")] 
    public string? Password { get; set; } = null;
}
