using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Movimientos.Models
{
    /// <summary>
    /// Representa un pago realizado con tarjeta asociado a un movimiento.
    /// Incluye información sobre el número de tarjeta, la cantidad y el comercio donde se realizó el pago.
    /// </summary>
    /// <remarks>
    /// Esta clase es utilizada para registrar un pago realizado con tarjeta de crédito o débito, incluyendo el número de tarjeta, la cantidad pagada y el nombre del comercio.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class PagoConTarjeta
    {
        /// <summary>
        /// El número de la tarjeta utilizada para realizar el pago.
        /// Este campo es obligatorio.
        /// </summary>
        [Required]
        public string NumeroTarjeta { get; set; }

        /// <summary>
        /// La cantidad pagada en el comercio.
        /// Este campo debe estar en el rango de 1 a 10,000.
        /// </summary>
        [Range(1, 10000, ErrorMessage = "La cantidad debe estar entre 1 y 10000")]
        public decimal Cantidad { get; set; }

        /// <summary>
        /// El nombre del comercio donde se realizó el pago.
        /// Este campo es obligatorio y tiene un límite de 100 caracteres.
        /// </summary>
        [Required]
        [MaxLength(100, ErrorMessage = "El nombre del comercio no puede tener más de 100 caracteres")]
        public string NombreComercio { get; set; }
    }
}
