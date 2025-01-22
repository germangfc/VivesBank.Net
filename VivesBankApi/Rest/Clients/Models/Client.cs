using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Clients.Models;
[Table("Clients")]
public class Client
{
    [Key]
    public String Id { get; set; } = GuuidGenerator.GenerateHash();
    
    [Required]
    [JsonProperty("userId")]
    [Length(9, 9, ErrorMessage = "The userId must be a guuid")]
    public String UserId { get; set; }
    
    [Required]
    [JsonProperty("fullName")]
    [MaxLength(100, ErrorMessage = "The name must be at most 100 characters long")]
    [MinLength(3, ErrorMessage = "The name must be at least 3 characters long")]
    public String FullName { get; set; }
    
    [Required]
    [JsonProperty("address")]
    [MaxLength(200, ErrorMessage = "The address must be at most 200 characters long")]
    [MinLength(5, ErrorMessage = "The address must be at least 5 characters long")]
    public String Adress { get; set; }

    [Required] 
    [JsonProperty("photo")]
    public String Photo { get; set; } = "defaultId.png";

    [Required]
    [JsonProperty("idPhoto")]
    public String PhotoDni { get; set; } = "default.png";
    
    [Required]
    [JsonProperty("accounts")]
    public List<String> AccountsIds { get; set; } = new();
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; } = false;
}