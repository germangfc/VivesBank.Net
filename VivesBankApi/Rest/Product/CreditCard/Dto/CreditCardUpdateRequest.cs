using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

/// <summary>
/// Representa la solicitud para actualizar una tarjeta de crédito existente.
/// Esta clase permite modificar los datos de una tarjeta de crédito, como el PIN.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardUpdateRequest
{
    /// <summary>
    /// El PIN de la tarjeta de crédito. Debe ser exactamente de 4 caracteres.
    /// Este valor se puede actualizar para la tarjeta existente.
    /// </summary>
    [StringLength(4, MinimumLength = 4, ErrorMessage = "The pin must be of 4 character")]
    public string Pin { get; set; }
}
