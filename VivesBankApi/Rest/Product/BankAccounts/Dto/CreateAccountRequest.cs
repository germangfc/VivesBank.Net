using System.ComponentModel.DataAnnotations;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

public class CreateAccountRequest
{
    [Required(ErrorMessage = "The product name must be specified")]
    public String ProductName { get; set; }
    [Required(ErrorMessage = "The Account type must be specified")]
    public AccountType AccountType { get; set; }
}