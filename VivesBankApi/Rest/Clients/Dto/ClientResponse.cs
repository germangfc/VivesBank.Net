using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientResponse
{
    [Required]
    public string Id { get; set; }
    [Required]
    public string UserId { get; set; }
    [Required]
    public string Address { get; set; } 
    [Required]
    public string Fullname { get; set; }
    [Required]
    public List<String?> Accounts { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
}