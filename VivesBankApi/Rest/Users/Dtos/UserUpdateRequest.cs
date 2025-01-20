using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserUpdateRequest
{
    [Required]
    [Length(9, 9, ErrorMessage = "The username must be a DNI")]
    [JsonProperty("username")] 
    public string Username { get; set; }
    
    [Required]
    [MinLength(8, ErrorMessage = "The password must be at least 5 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")] 
    public string Password { get; set; }

    [Required]
    [JsonProperty("role")]
    public string Role { get; set; }
}
