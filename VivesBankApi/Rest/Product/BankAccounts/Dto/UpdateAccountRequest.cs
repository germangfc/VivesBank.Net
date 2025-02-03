using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

/// <summary>
/// Representa la solicitud para actualizar una cuenta bancaria existente.
/// </summary>
/// <remarks>
/// Esta clase contiene los campos necesarios para actualizar una cuenta bancaria, como el ID del producto, 
/// el ID del cliente, el IBAN, la tarjeta asociada (si existe), el saldo de la cuenta y el tipo de cuenta.
/// Es utilizada para recibir los datos de entrada en el proceso de actualización de una cuenta bancaria.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class UpdateAccountRequest
{
    /// <summary>
    /// ID del producto asociado a la cuenta bancaria.
    /// </summary>
    /// <exception cref="ValidationException">Si el ID del producto no está especificado.</exception>
    [Required(ErrorMessage = "The id of the product must be specified")]
    public string ProductID { get; set; }

    /// <summary>
    /// ID del cliente propietario de la cuenta bancaria.
    /// </summary>
    /// <exception cref="ValidationException">Si el ID del cliente no está especificado.</exception>
    [Required(ErrorMessage = "The id of the client who owns the account must be specified")]
    public string ClientID { get; set; }

    /// <summary>
    /// ID de la tarjeta asociada a la cuenta. Este campo puede ser nulo si no está asociada ninguna tarjeta.
    /// </summary>
    /// <remarks>
    /// Este campo se utiliza para asociar una tarjeta a la cuenta, pero si no se desea asociar ninguna tarjeta, 
    /// puede dejarse vacío. Este campo es opcional.
    /// </remarks>
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? TarjetaId { get; set; }

    /// <summary>
    /// IBAN de la cuenta bancaria. Este campo es obligatorio para identificar la cuenta a actualizar.
    /// </summary>
    public string IBAN { get; set; }

    /// <summary>
    /// Saldo actual de la cuenta bancaria. Este campo debe ser especificado al actualizar la cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el saldo de la cuenta no está especificado.</exception>
    [Required(ErrorMessage = "The account balance must be specified")]
    public decimal Balance { get; set; } = 0;

    /// <summary>
    /// Tipo de cuenta bancaria (por ejemplo, cuenta de ahorros o cuenta corriente).
    /// </summary>
    /// <exception cref="ValidationException">Si el tipo de cuenta no está especificado.</exception>
    [Required(ErrorMessage = "The account type must be specified")]
    public AccountType AccountType { get; set; }
}
