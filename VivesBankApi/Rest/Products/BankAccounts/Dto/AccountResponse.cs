using System.ComponentModel.DataAnnotations;
using VivesBankApi.Products.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Products.BankAccounts.Dto;

public class AccountResponse
{
    [Required(ErrorMessage = "The IBAN of the account must be specified")]
    public string IBAN { get; set; }
    [Required(ErrorMessage = "The id of the client who owns the account must be specified")]
    public String clientID { get; set; }
    [Required (ErrorMessage = "The id of the product must be specified")]
    public String productID { get; set; }
    [Required (ErrorMessage = "The account type must be specified")]
    public AccountType AccountType { get; set; }
    [Required (ErrorMessage = "The account interest rate must be specified")]
    public double InterestRate { get; set; }
    
}