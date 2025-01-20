using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientUpdateRequest
{

    public String FullName { get; set; }
    
    public String Address { get; set; }
}