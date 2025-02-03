using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Base.Dto;

/// <summary>
/// Representa la solicitud de actualización de un producto.
/// </summary>
public class ProductUpdateRequest
{
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y debe contener un nombre de producto con una longitud mínima de 3 caracteres y máxima de 100 caracteres.
    /// </remarks>
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; }
    
    /// <summary>
    /// Tipo del producto.
    /// </summary>
    /// <remarks>
    /// Este campo es opcional. Representa el tipo del producto.
    /// </remarks>
    public string Type { get; set; }
}
