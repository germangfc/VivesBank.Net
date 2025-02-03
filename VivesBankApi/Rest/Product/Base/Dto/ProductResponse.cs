using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Base.Dto;

/// <summary>
/// Representa la respuesta de un producto.
/// </summary>
public class ProductResponse
{
    /// <summary>
    /// Identificador único del producto.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y representa el ID único del producto.
    /// </remarks>
    [Required]
    public string Id { get; set; }
    
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y debe contener un nombre de producto con una longitud máxima de 100 caracteres.
    /// </remarks>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    /// <summary>
    /// Tipo del producto.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y especifica el tipo del producto.
    /// </remarks>
    [Required]
    public string Type { get; set; }
    
    /// <summary>
    /// Fecha de creación del producto en formato string.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y representa la fecha y hora en que el producto fue creado.
    /// </remarks>
    [Required]
    public string CreatedAt { get; set; }
    
    /// <summary>
    /// Fecha de actualización del producto en formato string.
    /// </summary>
    /// <remarks>
    /// Este campo es obligatorio y representa la fecha y hora en que el producto fue actualizado.
    /// </remarks>
    [Required]
    public string UpdatedAt { get; set; }
}
