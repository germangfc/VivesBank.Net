using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace VivesBankApi.Rest.Product.BankAccounts.Dto;

public class UpdateAccountRequest
{

    [Required (ErrorMessage = "The id of the product must be specified")]
    public String ProductID { get; set; }
    
    [Required(ErrorMessage = "The id of the client who owns the account must be specified")]
    public String ClientID { get; set; }
    
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public String? TarjetaId { get; set; }

    public string IBAN { get; set; }

    [Required(ErrorMessage = "The account balance must be specified")]
    public decimal Balance { get; set; } = 0;
    
    [Required (ErrorMessage = "The account type must be specified")]
    public AccountType AccountType { get; set; }

}