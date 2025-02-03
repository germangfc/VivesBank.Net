using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models
{
    /// <summary>
    /// Representa un ingreso de nómina que se realiza desde una cuenta de origen hacia una cuenta de destino.
    /// La clase incluye los detalles de la cuenta de origen, la cuenta de destino, la cantidad y la empresa que realiza el pago.
    /// </summary>
    /// <remarks>
    /// Esta clase es utilizada para gestionar los ingresos de nómina en el sistema, asegurando que la cantidad esté dentro de un rango válido y 
    /// que los campos como el nombre de la empresa y el CIF sean correctos.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class IngresoDeNomina
    {
        /// <summary>
        /// El IBAN de la cuenta de destino donde se depositará la nómina.
        /// </summary>
        [Required]
        public string IbanDestino { get; set; }

        /// <summary>
        /// El IBAN de la cuenta de origen desde la cual se transferirá el dinero.
        /// </summary>
        [Required]
        public string IbanOrigen { get; set; }

        /// <summary>
        /// La cantidad de dinero a transferir en cada ingreso de nómina.
        /// Debe estar dentro del rango de 1 a 10000.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")]
        public decimal Cantidad { get; set; }

        /// <summary>
        /// El nombre de la empresa que está realizando el pago de la nómina.
        /// El nombre no puede exceder los 100 caracteres.
        /// </summary>
        [MaxLength(100, ErrorMessage = "El nombre de la empresa no puede tener más de 100 caracteres")]
        public string NombreEmpresa { get; set; }

        /// <summary>
        /// El CIF (Código de Identificación Fiscal) de la empresa que realiza el ingreso de nómina.
        /// Este campo puede tener validación para asegurar que el formato del CIF sea correcto.
        /// </summary>
        //[RegularExpression("^[A-Z0-9]{9}$", ErrorMessage = "El CIF debe tener 9 caracteres alfanuméricos")]
        public string CifEmpresa { get; set; }
    }
}
