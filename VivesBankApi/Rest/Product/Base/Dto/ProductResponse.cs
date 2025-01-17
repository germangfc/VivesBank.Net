using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.Base.Dto;

public class ProductResponse
{
    

    
    
    [Required]
    [MaxLength(100)]
    public String Name { get; set; }
    
    [Required]
    public String Type { get; set; }
    
    [Required]
    public String CreatedAt { get; set; }
    
    [Required]
    public String UpdatedAt { get; set; }
}