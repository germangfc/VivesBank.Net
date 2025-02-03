using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

/// <summary>
/// Representa la respuesta completa de una cuenta bancaria.
/// </summary>
/// <remarks>
/// Esta clase incluye todos los detalles asociados con una cuenta bancaria, como el IBAN, el cliente propietario, el tipo de cuenta, el saldo, 
/// las tasas de interés y la fecha de creación y actualización. Se utiliza para devolver la información completa de una cuenta bancaria.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class AccountCompleteResponse
{
    /// <summary>
    /// Identificador único de la cuenta.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// El IBAN de la cuenta bancaria.
    /// </summary>
    /// <exception cref="ValidationException">Si el IBAN no está especificado.</exception>
    [Required(ErrorMessage = "The IBAN of the account must be specified")]
    public string IBAN { get; set; }

    /// <summary>
    /// Identificador del cliente propietario de la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el id del cliente no está especificado.</exception>
    [Required(ErrorMessage = "The id of the client who owns the account must be specified")]
    public string ClientID { get; set; }

    /// <summary>
    /// Identificador del producto asociado a la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el id del producto no está especificado.</exception>
    [Required(ErrorMessage = "The id of the product must be specified")]
    public string productID { get; set; }

    /// <summary>
    /// Tipo de cuenta (ahorros, corriente, etc.).
    /// </summary>
    /// <exception cref="ValidationException">Si el tipo de cuenta no está especificado.</exception>
    [Required(ErrorMessage = "The account type must be specified")]
    public AccountType AccountType { get; set; }

    /// <summary>
    /// La tasa de interés asociada a la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si la tasa de interés no está especificada.</exception>
    [Required(ErrorMessage = "The account interest rate must be specified")]
    public double InterestRate { get; set; }

    /// <summary>
    /// El saldo actual de la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el saldo no está especificado.</exception>
    [Required(ErrorMessage = "The account balance must be specified")]
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// El identificador de la tarjeta asociada a la cuenta (opcional).
    /// </summary>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? TarjetaId { get; set; }

    /// <summary>
    /// Fecha en que se creó la cuenta.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha en que se actualizó la cuenta por última vez.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Indica si la cuenta ha sido eliminada.
    /// </summary>
    public bool IsDeleted { get; set; }
}
