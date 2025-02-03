using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Movimientos.Models
{
    /// <summary>
    /// Representa una domiciliación bancaria, que se puede ejecutar de forma periódica para transferir fondos entre cuentas.
    /// </summary>
    /// <remarks>
    /// La domiciliación incluye la información de las cuentas origen y destino, así como la cantidad a transferir.
    /// Además, se almacena la frecuencia con la que se ejecuta la domiciliación (por ejemplo, diaria, semanal, mensual).
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class Domiciliacion
    {
        /// <summary>
        /// Identificador único de la domiciliación en la base de datos (representado como un ObjectId en MongoDB).
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Un identificador único generado para cada domiciliación, utilizado en el sistema.
        /// </summary>
        public string Guid { get; set; } = GuuidGenerator.GenerateHash();

        /// <summary>
        /// El identificador único del cliente asociado a esta domiciliación.
        /// </summary>
        public string ClienteGuid { get; set; }

        /// <summary>
        /// El IBAN de la cuenta origen desde la cual se transferirán los fondos.
        /// </summary>
        [Required]
        public string IbanOrigen { get; set; }

        /// <summary>
        /// El IBAN de la cuenta destino a la cual se transferirán los fondos.
        /// </summary>
        [Required]
        public string IbanDestino { get; set; }

        /// <summary>
        /// El monto a transferir en cada ejecución de la domiciliación.
        /// Debe estar en el rango de 1 a 10000.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")]
        public decimal Cantidad { get; set; }

        /// <summary>
        /// El nombre del acreedor al que se realizarán los pagos.
        /// </summary>
        [MaxLength(100, ErrorMessage = "El nombre del acreedor no puede tener más de 100 caracteres")]
        public string NombreAcreedor { get; set; }

        /// <summary>
        /// La fecha en la que inicia la domiciliación. Si no se especifica, se establece como la fecha y hora actual.
        /// </summary>
        [NotNull]
        public DateTime FechaInicio { get; set; } = DateTime.Now;

        /// <summary>
        /// La periodicidad con la que se ejecuta la domiciliación (DIARIA, SEMANAL, MENSUAL, ANUAL).
        /// El valor por defecto es MENSUAL.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Periodicidad Periodicidad { get; set; } = Periodicidad.MENSUAL;

        /// <summary>
        /// Indica si la domiciliación está activa o no. Si está desactivada, no se ejecutará.
        /// </summary>
        public bool Activa { get; set; } = true;

        /// <summary>
        /// Fecha de la última ejecución de la domiciliación. Inicialmente se establece con la fecha y hora actual.
        /// </summary>
        public DateTime UltimaEjecucion { get; set; } = DateTime.Now;

        // [JsonPropertyName("id")]
        // public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal
    }
}
