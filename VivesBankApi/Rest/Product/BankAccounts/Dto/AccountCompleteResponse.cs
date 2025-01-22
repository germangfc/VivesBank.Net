using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

public class AccountCompleteResponse
{
    public String Id { get; set; }
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

    [Required(ErrorMessage = "The account balance must be specified")]
    public decimal Balance { get; set; } = 0;
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public String? TarjetaId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
}