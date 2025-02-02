using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardUpdateRequest
{
    [StringLength(4, MinimumLength = 4)]
    public String Pin { get; set; }
    
    
}