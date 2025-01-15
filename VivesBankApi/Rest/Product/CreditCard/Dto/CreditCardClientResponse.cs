using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardClientResponse
{
    
    [Required]
    public String AccountId { get; set; }
    
    [Required]
    public String CardNumber { get; set; }
    
    [Required]
    public String Pin { get; set; }
    
    [Required]
    public String Cvc { get; set; }
    
    public JSType.Date ExpirationDate { get; set; }

    
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
}