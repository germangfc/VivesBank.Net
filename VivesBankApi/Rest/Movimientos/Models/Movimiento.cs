using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Models;

public class Movimiento
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    
    public string Guid { get; set; } // = IdGenerator.GenerateGuid();
    
    public string ClienteGuid { get; set; }
    
    public Domiciliacion Domiciliacion { get; set; }
    
    public IngresoDeNomina IngresoDeNomina;
    
    public PagoConTarjeta PagoConTarjeta;
    
    public Transferencia Transferencia;
    
    [JsonPropertyName("createdAt")] 
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")] 
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("id")]
    public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal

    [JsonPropertyName("isDeleted")] 
    public bool IsDeleted { get; set; } = false;
}


