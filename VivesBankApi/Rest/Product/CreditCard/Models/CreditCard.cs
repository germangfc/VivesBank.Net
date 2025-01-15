using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace VivesBankApi.Rest.Product.CreditCard.Models;

[Table("CreditCards")]
public class CreditCard
{
    [Key]
    public String Id { get; set; }
    
    [Required]
    public String AccountId { get; set; }
    
    [Required]
    public String CardNumber { get; set; }
    
    [Required]
    public String Pin { get; set; }
    
    [Required]
    public String Cvc { get; set; }
    
    [Required]
    public JSType.Date ExpirationDate { get; set; }
    
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
}