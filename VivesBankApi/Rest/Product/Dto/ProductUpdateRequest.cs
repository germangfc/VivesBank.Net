using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Dto;

public class ProductUpdateRequest
{
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; }
}