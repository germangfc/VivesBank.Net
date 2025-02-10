using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

/// <summary>
/// Representa la solicitud para crear una nueva tarjeta de crédito.
/// Esta clase contiene los datos necesarios para la creación de una tarjeta de crédito asociada a una cuenta.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardRequest
{
    /// <summary>
    /// El PIN de la tarjeta de crédito. Debe ser exactamente de 4 caracteres.
    /// </summary>
    [Required]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "The pin must be of 4 characters")]
    public string Pin { get; set; }
    
    /// <summary>
    /// El IBAN de la cuenta asociada a la tarjeta de crédito. 
    /// Es un identificador único de la cuenta bancaria para la creación de la tarjeta.
    /// </summary>
    [Required]
    public string AccountIban { get; set; }
}
