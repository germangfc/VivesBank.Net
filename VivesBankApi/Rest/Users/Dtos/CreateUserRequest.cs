using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class CreateUserRequest
{
    [Required]
    [MinLength(5)]
    [MaxLength(50)]
    [JsonProperty("username")]
    public String Username { get; set; }
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    [JsonProperty("username")]
    public String Password { get; set; }
    
    [Required]
    public String Role { get; set; }
}