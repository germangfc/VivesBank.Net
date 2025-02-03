using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Base.Dto;

/// <summary>
/// Representa una solicitud para crear un nuevo producto.
/// </summary>
public class ProductCreateRequest
{
    /// <summary>
    /// Nombre del producto.
    /// </summary>
    /// <remarks>
    /// El nombre debe tener una longitud mínima de 3 caracteres y una longitud máxima de 100 caracteres.
    /// </remarks>
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; }
    
    /// <summary>
    /// Tipo del producto.
    /// </summary>
    /// <remarks>
    /// El tipo de producto es un campo obligatorio.
    /// </remarks>
    [Required]
    public string Type { get; set; }
}
