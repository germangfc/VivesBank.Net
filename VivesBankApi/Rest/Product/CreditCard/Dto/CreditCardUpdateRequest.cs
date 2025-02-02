using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardUpdateRequest
{
    [StringLength(4, MinimumLength = 4, ErrorMessage = "The pin must be of 4 character")]
    public String Pin { get; set; }
    
    
}