using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardRequest
{
    [Required]
    public String Pin { get; set; }
    
    [Required]
    public String AccountIban { get; set; }
}