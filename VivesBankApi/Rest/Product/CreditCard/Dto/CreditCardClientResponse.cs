using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

/// <summary>
/// Representa la respuesta de una tarjeta de crédito asociada a un cliente, 
/// utilizada para mostrar información detallada sobre la tarjeta en el contexto de un cliente.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardClientResponse
{
    /// <summary>
    /// Identificador único de la tarjeta de crédito.
    /// </summary>
    [Required]
    public string Id { get; set; }
    
    /// <summary>
    /// Identificador único de la cuenta asociada a la tarjeta de crédito.
    /// </summary>
    [Required]
    public string AccountId { get; set; }
    
    /// <summary>
    /// Número de la tarjeta de crédito. Este valor debe ser único.
    /// </summary>
    [Required]
    public string CardNumber { get; set; }
    
    /// <summary>
    /// PIN asociado a la tarjeta de crédito. Usado para la autenticación del cliente.
    /// </summary>
    [Required]
    public string Pin { get; set; }
    
    /// <summary>
    /// Código de seguridad (CVC) de la tarjeta de crédito.
    /// </summary>
    [Required]
    public string Cvc { get; set; }
    
    /// <summary>
    /// Fecha de expiración de la tarjeta de crédito en formato "MM/AAAA".
    /// </summary>
    [Required]
    public string ExpirationDate { get; set; }

    /// <summary>
    /// Fecha de creación de la tarjeta de crédito.
    /// </summary>
    public DateTime CreatedAt = DateTime.Now;

    /// <summary>
    /// Fecha de la última actualización de la tarjeta de crédito.
    /// </summary>
    public DateTime UpdatedAt = DateTime.Now;
    
    /// <summary>
    /// Indica si la tarjeta de crédito está marcada como eliminada.
    /// </summary>
    public bool IsDeleted { get; set; }
}
