using System.ComponentModel.DataAnnotations.Schema;
using VivesBankApi.Products.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Products.BankAccounts.Models;
[Table("BankAccounts")]
public class Account
{
    public String Id { get; set; }
    public String ProductId { get; set; }
    public String ClientId { get; set; }
    public String? TarjetaId { get; set; }
    public String IBAN { get; set; }
    public Decimal Balance { get; set; } = 0;
    public AccountType AccountType { get; set; }
    public double InterestRate => AccountType.GetInterestRate();
}