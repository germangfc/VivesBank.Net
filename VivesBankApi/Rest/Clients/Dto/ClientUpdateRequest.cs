using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientUpdateRequest
{
    [StringLength(80, MinimumLength = 3, ErrorMessage = "FullName must be between 3 and 80 characters.")]
    public String FullName { get; set; }
    [StringLength(80, MinimumLength = 3, ErrorMessage = "Address must be between 3 and 80 characters.")]
    public String Address { get; set; }
}