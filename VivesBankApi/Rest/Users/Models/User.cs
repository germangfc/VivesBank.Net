using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Users.Models;

[Table("Users")]
public class User
{
    [Key] 
    [JsonProperty()]
    public String Id { get; set; } = GuuidGenerator.GenerateHash();
    
    [Required]
    [MinLength(5)]
    [MaxLength(50)]
    [JsonProperty("username")]
    public String Username { get; set; }
    
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    [JsonProperty("password")]
    public String Password { get; set; }
    
    [Required]
    [JsonProperty("role")]
    public Role Role { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    public User() { }
    public User(String username, String password, Role role)
    {
        this.Username = username;
        this.Password = password;
        this.Role = role;
    }
    
}

public enum Role
{
 User, Admin, SuperAdmin,
} 