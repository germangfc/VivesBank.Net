using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Models;
[Table("BankAccounts")]
public class Account
{
    [Key]
    public String Id { get; set; }
    [Required]
    public String ProductId { get; set; }
    [Required]
    public String ClientId { get; set; }
    [Required]
    public String? TarjetaId { get; set; }
    [Required]
    public String IBAN { get; set; }
    [Required]
    public Decimal Balance { get; set; } = 0;
    [Required]
    public AccountType AccountType { get; set; }
    public double InterestRate => AccountType.GetInterestRate();
    public DateTime CreatedAt = DateTime.Now;
    public DateTime UpdatedAt = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
}