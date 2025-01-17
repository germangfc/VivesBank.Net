using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Rest.Product.CreditCard.Models;

[Table("CreditCards")]
public class CreditCard
{
    [Key] public String Id { get; set; } = GuuidGenerator.GenerateHash();
    
    [Required]
    public String AccountId { get; set; }
    
    [Required]
    public String CardNumber { get; set; }
    
    [Required]
    public String Pin { get; set; }
    
    [Required]
    public String Cvc { get; set; }
    
    [Required]
    public DateOnly ExpirationDate { get; set; }
    
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
}