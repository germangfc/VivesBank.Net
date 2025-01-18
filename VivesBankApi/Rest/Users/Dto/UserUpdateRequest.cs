using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserUpdateRequest
{
    [Length(9, 9, ErrorMessage = "Must be a DNI")]
    [JsonProperty("dni")] 
    public string? Dni { get; set; } = null;
    
    [MinLength(8, ErrorMessage = "The password must be at least 5 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")] 
    public string? Password { get; set; } = null;

    [JsonProperty("role")]
    public string? Role { get; set; } = null;
}
