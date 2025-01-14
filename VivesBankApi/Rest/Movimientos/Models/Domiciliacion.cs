using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VivesBankApi.Rest.Movimientos.Models;

public class Domiciliacion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    
    public string Guid { get; set; } // = IdGenerator.GenerateGuid();

    public string ClienteGuid { get; set; }
    
    public string IbanOrigen { get; set; }
    
    public string IbanDestino { get; set; }
    
    public decimal Cantidad { get; set; }
    
    public string NombreAcreedor { get; set; }
    
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    
    [BsonRepresentation(BsonType.String)]
    public Periodicidad Periodicidad { get; set; } = Periodicidad.MENSUAL;

    public bool Activa { get; set; } = true;

    public DateTime UltimaEjecucion { get; set; } = DateTime.Now;
    
    [JsonPropertyName("id")]
    public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal

}