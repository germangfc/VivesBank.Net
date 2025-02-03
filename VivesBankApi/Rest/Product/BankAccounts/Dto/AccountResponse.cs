using System.ComponentModel.DataAnnotations;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

/// <summary>
/// Representa la respuesta de una cuenta bancaria con la información básica de la cuenta.
/// </summary>
/// <remarks>
/// Esta clase incluye los detalles esenciales de una cuenta bancaria, como el IBAN, el cliente propietario, el tipo de cuenta y la tasa de interés. 
/// Es utilizada para devolver la información más relevante de la cuenta bancaria.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class AccountResponse
{
    /// <summary>
    /// Identificador único de la cuenta bancaria.
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
    public string clientID { get; set; }

    /// <summary>
    /// Identificador del producto asociado a la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el id del producto no está especificado.</exception>
    [Required(ErrorMessage = "The id of the product must be specified")]
    public string productID { get; set; }
    
    /// <summary>
    /// Balance de la cuenta bancaria
    /// </summary>
    /// <exception cref="ValidationException">Si el balance de la cuenta no está especificado.</exception>
    [Required(ErrorMessage = "The balance of the account must be specified")]
    public Decimal Balance { get; set; }

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
}
