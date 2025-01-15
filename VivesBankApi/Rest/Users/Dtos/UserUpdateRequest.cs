using Newtonsoft.Json;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Dtos;

public class UserUpdateRequest
{
    [JsonProperty("username")]
    public String? Username = null;
    
    [JsonProperty("password")]
    public String? Password = null;
    
    [JsonProperty("role")]
    public String? Role = null;
    
}