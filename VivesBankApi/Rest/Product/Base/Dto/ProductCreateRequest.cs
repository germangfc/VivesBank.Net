using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Base.Dto;

public class ProductCreateRequest
{
    [Required]
    [MaxLength(100)]
    [MinLength(3)]
    public string Name { get; set; }
    
    [Required]
    public String Type { get; set; }
}