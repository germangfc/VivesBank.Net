using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.utils.GuuidGenerator;
using VivesBankApi.Utils.IbanGenerator;

namespace VivesBankApi.Rest.Product.BankAccounts.Models
{
    /// <summary>
    /// Representa una cuenta bancaria en el sistema.
    /// Esta clase contiene la información básica de una cuenta, incluyendo el ID de la cuenta, el producto, el cliente, el saldo, el tipo de cuenta y más.
    /// </summary>
    /// <remarks>
    /// Autor: Raúl Fernández, Javier Hernández, Samuel Cortés, Germán, Álvaro Herrero, Tomás
    /// Versión: 1.0
    /// </remarks>
    [Table("BankAccounts")]
    public class Account
    {
        /// <summary>
        /// Obtiene o establece el identificador único de la cuenta.
        /// </summary>
        /// <value>
        /// El identificador único generado automáticamente para la cuenta.
        /// </value>
        [Key]
        public String Id { get; set; } = GuuidGenerator.GenerateHash();

        /// <summary>
        /// Obtiene o establece el identificador del producto asociado a la cuenta.
        /// </summary>
        /// <value>
        /// El identificador del producto de la cuenta.
        /// </value>
        [Required]
        public String ProductId { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador del cliente que posee la cuenta.
        /// </summary>
        /// <value>
        /// El identificador del cliente que posee la cuenta.
        /// </value>
        [Required]
        public String ClientId { get; set; }

        /// <summary>
        /// Obtiene o establece el identificador de la tarjeta asociada a la cuenta (si existe).
        /// </summary>
        /// <value>
        /// El identificador de la tarjeta o null si no está asociada.
        /// </value>
        public String? TarjetaId { get; set; }

        /// <summary>
        /// Obtiene o establece el IBAN de la cuenta.
        /// </summary>
        /// <value>
        /// El IBAN de la cuenta, utilizado para la identificación internacional de la cuenta.
        /// </value>
        [Required]
        public String IBAN { get; set; }

        /// <summary>
        /// Obtiene o establece el saldo actual de la cuenta.
        /// </summary>
        /// <value>
        /// El saldo de la cuenta, representado como un valor decimal.
        /// </value>
        [Required]
        public Decimal Balance { get; set; } = 0;

        /// <summary>
        /// Obtiene o establece el tipo de cuenta (por ejemplo, de ahorros, estándar, etc.).
        /// </summary>
        /// <value>
        /// El tipo de cuenta, que determina las características y el interés aplicado a la cuenta.
        /// </value>
        [Required]
        public AccountType AccountType { get; set; }

        /// <summary>
        /// Obtiene la tasa de interés asociada al tipo de cuenta.
        /// </summary>
        /// <value>
        /// La tasa de interés que corresponde al tipo de cuenta asignado.
        /// </value>
        public double InterestRate => AccountType.GetInterestRate();

        /// <summary>
        /// Obtiene o establece la fecha de creación de la cuenta.
        /// </summary>
        /// <value>
        /// La fecha y hora en que se creó la cuenta, establecida por defecto en la fecha y hora actual en formato UTC.
        /// </value>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Obtiene o establece la fecha de la última actualización de la cuenta.
        /// </summary>
        /// <value>
        /// La fecha y hora en que la cuenta fue actualizada por última vez, establecida por defecto en la fecha y hora actual en formato UTC.
        /// </value>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la cuenta ha sido eliminada.
        /// </summary>
        /// <value>
        /// True si la cuenta ha sido eliminada, false si no.
        /// </value>
        public bool IsDeleted { get; set; } = false;
    }
}
