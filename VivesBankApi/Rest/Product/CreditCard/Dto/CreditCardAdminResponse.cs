using System.ComponentModel.DataAnnotations;

namespace VivesBankApi.Rest.Product.CreditCard.Dto;

public class CreditCardAdminResponse
{
    [Required]
    public String Id { get; set; }
    
    [Required]
    public String AccountId { get; set; }
    
    [Required]
    public String CardNumber { get; set; }
    
    [Required]
    public String ExpirationDate { get; set; }
    
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
}