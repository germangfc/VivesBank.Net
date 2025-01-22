using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientRequest
{
    [Required]
    [MaxLength(50, ErrorMessage = "The name must me at most 50 characters")]
    [MinLength(5, ErrorMessage = "The name must be at least 5 characters")]
    public String FullName { get; set; }
    [Required]
    [MaxLength(200, ErrorMessage = "The address must me at most 100 characters")]
    [MinLength(10, ErrorMessage = "The address must be at least 10 characters")]
    public String Address { get; set; }
}