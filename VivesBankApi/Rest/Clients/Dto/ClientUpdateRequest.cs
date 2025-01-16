using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientUpdateRequest
{
    [Required]
    public String FullName { get; set; }
    [Required]
    public String Address { get; set; }
}