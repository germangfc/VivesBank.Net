using Newtonsoft.Json;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserResponse
{
    [JsonProperty("id")]
    public String Id { get; set; }
    
    [JsonProperty("username")]
    public String Username { get; set; }
    
    [JsonProperty("role")]
    public String Role { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}