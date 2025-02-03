using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models
{
    /// <summary>
    /// Representa una transferencia de fondos entre cuentas bancarias.
    /// </summary>
    /// <remarks>
    /// Esta clase se utiliza para definir las características de una transferencia, como la cuenta origen, destino, cantidad y beneficiario.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class Transferencia
    {
        /// <summary>
        /// Representa la cuenta bancaria de origen de la transferencia.
        /// </summary>
        /// <remarks>Debe ser un IBAN válido de la cuenta origen.</remarks>
        [Required]
        public string IbanOrigen { get; set; }
        
        /// <summary>
        /// Representa la cuenta bancaria de destino de la transferencia.
        /// </summary>
        /// <remarks>Debe ser un IBAN válido de la cuenta destino.</remarks>
        [Required]
        public string IbanDestino { get; set; }
        
        /// <summary>
        /// Monto a transferir en la operación. 
        /// El valor debe estar entre 1 y 10000.
        /// </summary>
        /// <remarks>La cantidad transferida debe cumplir con el rango establecido.</remarks>
        [Required]
        [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")]
        public decimal Cantidad { get; set; }
        
        /// <summary>
        /// Nombre del beneficiario de la transferencia.
        /// </summary>
        /// <remarks>El nombre no puede superar los 100 caracteres.</remarks>
        [MaxLength(100, ErrorMessage = "El beneficiario no puede tener más de 100 caracteres")]
        public string NombreBeneficiario { get; set; }
        
        /// <summary>
        /// Identificador opcional del movimiento destino relacionado con la transferencia.
        /// </summary>
        /// <remarks>Este campo es opcional y se puede utilizar para asociar un movimiento previamente creado.</remarks>
        public string? MovimientoDestino { get; set; }
    }
}
