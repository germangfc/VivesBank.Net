using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardRequest
{
    [Required]
    [StringLength(4,MinimumLength = 4, ErrorMessage = "The pin must be of 4 characters")]
    public String Pin { get; set; }
    
    [Required]
    public String AccountIban { get; set; }
    
    public string CardNumber { get; set; }
}