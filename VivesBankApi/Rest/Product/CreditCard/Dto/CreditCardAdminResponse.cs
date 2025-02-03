using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

/// <summary>
/// Representa la respuesta de una tarjeta de crédito administrada, 
/// utilizada para mostrar información sobre una tarjeta en el contexto de un administrador.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardAdminResponse
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
