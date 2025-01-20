using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class LoginRequest
{
    [Required]
    [Length(9, 9, ErrorMessage = "Must be a DNI")]
    [JsonProperty("dni")]
    public String Dni { get; set; }
    [Required]
    [MinLength(8, ErrorMessage = "The password must be at least 5 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")]
    public String Password { get; set; }
    
    
}