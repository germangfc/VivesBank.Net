using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Movimientos.Models;

public class Domiciliacion
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public String Id { get; set; }
    
    public string Guid { get; set; } = GuuidGenerator.GenerateHash();

    public string ClienteGuid { get; set; }
    
    [Required]
    public string IbanOrigen { get; set; }
    
    [Required]
    public string IbanDestino { get; set; }
    
    [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")] 
    public decimal Cantidad { get; set; }
    
    [MaxLength(100, ErrorMessage = "El nombre del acreedor no puede tener más de 100 caracteres")]
    public string NombreAcreedor { get; set; }
    
    [NotNull]
    public DateTime FechaInicio { get; set; } = DateTime.Now;
    
    [BsonRepresentation(BsonType.String)]
    public Periodicidad Periodicidad { get; set; } = Periodicidad.MENSUAL;

    public bool Activa { get; set; } = true;

    public DateTime UltimaEjecucion { get; set; } = DateTime.Now;
    
    [JsonPropertyName("id")]
    public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal

}