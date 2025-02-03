using Newtonsoft.Json;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserResponse
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    [JsonProperty("id")]
    public String Id { get; set; }
    
    /// <summary>
    /// Documento Nacional de Identidad (DNI) del usuario.
    /// </summary>
    [JsonProperty("dni")]
    public String Dni { get; set; }
    
    /// <summary>
    /// Rol asignado al usuario.
    /// </summary>
    [JsonProperty("role")]
    public String Role { get; set; }
    
    /// <summary>
    /// Fecha de creación del usuario.
    /// </summary>
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Fecha de la última actualización del usuario.
    /// </summary>
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Indica si el usuario está marcado como eliminado.
    /// </summary>
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}
