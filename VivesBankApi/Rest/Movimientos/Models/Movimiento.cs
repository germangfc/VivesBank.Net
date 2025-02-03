using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using VivesBankApi.utils.GuuidGenerator;
namespace VivesBankApi.Rest.Movimientos.Models
{
    /// <summary>
    /// Representa un movimiento realizado en el sistema, que puede estar asociado a diferentes tipos de operaciones financieras.
    /// Este movimiento puede estar relacionado con domiciliaciones, ingresos de nómina, pagos con tarjeta o transferencias.
    /// </summary>
    /// <remarks>
    /// La clase contiene información clave sobre el movimiento, incluyendo el identificador del cliente, el tipo de operación y las fechas asociadas.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class Movimiento
    {
        /// <summary>
        /// El identificador único del movimiento en la base de datos.
        /// Este campo es de tipo ObjectId, utilizado por MongoDB.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// El GUID único del movimiento. Se genera automáticamente cuando se crea un nuevo movimiento.
        /// </summary>
        public string Guid { get; set; } = GuuidGenerator.GenerateHash();

        /// <summary>
        /// El identificador único del cliente que ha realizado el movimiento.
        /// Este campo se asocia al cliente en el sistema.
        /// </summary>
        public string ClienteGuid { get; set; }

        /// <summary>
        /// Representa una domiciliación asociada a este movimiento, si corresponde.
        /// Es una relación opcional, ya que no todos los movimientos son domiciliaciones.
        /// </summary>
        public Domiciliacion? Domiciliacion { get; set; }

        /// <summary>
        /// Representa un ingreso de nómina asociado a este movimiento, si corresponde.
        /// Es una relación opcional, ya que no todos los movimientos son ingresos de nómina.
        /// </summary>
        public IngresoDeNomina? IngresoDeNomina { get; set; }

        /// <summary>
        /// Representa un pago con tarjeta asociado a este movimiento, si corresponde.
        /// Es una relación opcional, ya que no todos los movimientos son pagos con tarjeta.
        /// </summary>
        public PagoConTarjeta? PagoConTarjeta { get; set; }

        /// <summary>
        /// Representa una transferencia asociada a este movimiento, si corresponde.
        /// Es una relación opcional, ya que no todos los movimientos son transferencias.
        /// </summary>
        public Transferencia? Transferencia { get; set; }

        /// <summary>
        /// La fecha y hora en que se creó el movimiento.
        /// </summary>
        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// La fecha y hora en que se actualizó por última vez el movimiento.
        /// </summary>
        [JsonPropertyName("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Indica si el movimiento ha sido marcado como eliminado.
        /// Este campo es útil para realizar un "soft delete" (eliminación lógica) en lugar de eliminar físicamente el registro.
        /// </summary>
        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        // [JsonPropertyName("id")]
        // public string Get_Id => Id.ToString(); // equivale a ToHexString, en C# devuelve representación hexadecimal
    }
}