using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Users.Models;

[Table("Users")]
public class User
{
    [Key] 
    [JsonProperty("id")] 
    public String Id { get; set; } = GuuidGenerator.GenerateHash();

    [Required]
    [Length(9, 9, ErrorMessage = "The username must be a DNI")]
    [JsonProperty("username")]
    public String Username { get; set; }
    
    [Required]
    [JsonProperty("password")]
    public String Password { get; set; }
    
    [Required]
    [JsonProperty("role")]
    [JsonConverter(typeof(StringEnumConverter))]
    public Role Role { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("updatedAt")] 
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; 
    
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}

public enum Role
{
 User, Admin, SuperAdmin,
} 