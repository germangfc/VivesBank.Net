using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientRequest
{
    [Required]
    public String FullName { get; set; }
    [Required]
    public String Photo { get; set; }
    [Required]
    public String PhotoDni { get; set; }
    [Required]
    public String Address { get; set; }
    [Required]
    public String UserId { get; set; }
    
}