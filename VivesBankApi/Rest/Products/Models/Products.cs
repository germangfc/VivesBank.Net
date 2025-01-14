using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.utils.GuuidGenerator;

namespace VivesBankApi.Products.Models;
[Table("Products")]
public class Products
{
    [Key]
    public String Id { get; set; }
    [Required]
    [MaxLength(100)]
    public String Name { get; set; }
    [Required]
    public Type ProductType { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now; 
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    [Required]
    public bool IsDeleted { get; set; }= false;

    public Products(String name, Type productType)
    {
        Id = GuuidGenerator.GenerateHash();
        Name = name;
        ProductType = productType;
    }

    public enum Type
    {
        BankAccount,
        CreditCard,
    }
}