using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VivesBankApi.Rest.Users.Models;

[Table("User")]
public class User
{
    [Key]
    public String Id { get; set; }
    
    [Required]
    [MinLength(5)]
    [MaxLength(50)]
    public String Username { get; set; }
    
    [Required]
    [MinLength(8)]
    [MaxLength(50)]
    public String Password { get; set; }
    
    [Required]
    public Role Role { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public bool IsDeleted { get; set; } = false;

    public User(String id, String username, String password, Role role)
    {
        this.Id = id;
        this.Username = username;
        this.Password = password;
        this.Role = role;
    }
    
}

public enum Role
{
 User, Admin, SuperAdmin,
} 