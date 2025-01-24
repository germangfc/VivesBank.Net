using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Users.Dtos;

public class UpdatePasswordRequest
{
    [MinLength(8, ErrorMessage = "The password must be at least 5 characters long")]
    [MaxLength(50, ErrorMessage = "The password must be at most 50 characters long")]
    [JsonProperty("password")] 
    public string? Password { get; set; } = null;
}