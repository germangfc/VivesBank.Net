using System.ComponentModel.DataAnnotations;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

/// <summary>
/// Representa la solicitud para crear una nueva cuenta bancaria.
/// </summary>
/// <remarks>
/// Esta clase contiene los campos necesarios para crear una cuenta bancaria, como el nombre del producto y el tipo de cuenta.
/// Es utilizada para recibir los datos de entrada en el proceso de creación de una nueva cuenta bancaria.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class CreateAccountRequest
{
    /// <summary>
    /// Nombre del producto asociado a la nueva cuenta.
    /// </summary>
    /// <exception cref="ValidationException">Si el nombre del producto no está especificado.</exception>
    [Required(ErrorMessage = "The product name must be specified")]
    public string ProductName { get; set; }

    /// <summary>
    /// Tipo de cuenta que se está creando (ahorros, corriente, etc.).
    /// </summary>
    /// <exception cref="ValidationException">Si el tipo de cuenta no está especificado.</exception>
    [Required(ErrorMessage = "The Account type must be specified")]
    public AccountType AccountType { get; set; }
}
