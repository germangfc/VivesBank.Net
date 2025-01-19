using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Clients.Dto;

public class ClientResponse
{
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("userId")]
    public string UserId { get; set; }
    
    [JsonProperty("address")]
    public string Address { get; set; } 
    
    [JsonProperty("fullname")]
    public string Fullname { get; set; }
    
    [JsonProperty("dniPhoto")]
    public string DniPhoto { get; set; }
    
    [JsonProperty("photo")]
    public string Photo { get; set; }
    
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    
    [JsonProperty("isDeleted")]
    public bool IsDeleted { get; set; }
    
}