using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserUpdateRequest
{
    /// <summary>
    /// Documento Nacional de Identidad (DNI) del usuario. Debe tener una longitud exacta de 9 caracteres.
    /// </summary>
    [Length(9, 9, ErrorMessage = "Must be a DNI")]
    [JsonProperty("dni")] 
    public string? Dni { get; set; } = null;
    
    /// <summary>
    /// Contraseña del usuario. Debe tener una longitud mínima de 8 caracteres y máxima de 50.
    /// </summary>
    [MinLength(8, ErrorMessage = "The password must be at least 5 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")] 
    public string? Password { get; set; } = null;

    /// <summary>
    /// Rol asignado al usuario. Puede ser modificado en la solicitud de actualización.
    /// </summary>
    [JsonProperty("role")]
    public string? Role { get; set; } = null;
    
    /// <summary>
    /// Indica si el usuario está marcado como eliminado.
    /// </summary>
    public bool IsDeleted { get; set; }
}
