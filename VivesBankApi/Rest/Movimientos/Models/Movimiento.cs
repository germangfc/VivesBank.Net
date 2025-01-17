using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Movimientos.Models;

public class Movimiento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } 

    public string Guid { get; set; } = GuuidGenerator.GenerateHash();
    
    public string ClienteGuid { get; set; }
    
    public Domiciliacion? Domiciliacion { get; set; }
    
    public IngresoDeNomina? IngresoDeNomina { get; set; }
    
    public PagoConTarjeta? PagoConTarjeta { get; set; }
    
    public Transferencia? Transferencia { get; set; }
    
    [JsonPropertyName("createdAt")] 
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")] 
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("isDeleted")] 
    public bool IsDeleted { get; set; } = false;
    
    // [JsonPropertyName("id")]
    // public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal

}


