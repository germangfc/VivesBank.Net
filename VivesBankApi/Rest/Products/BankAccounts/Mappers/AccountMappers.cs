using VivesBankApi.Products.BankAccounts.Models;
using VivesBankApi.Rest.Products.BankAccounts.Dto;

namespace VivesBankApi.Rest.Products.BankAccounts.Mappers;

public static class AccountMappers
{
    public static AccountResponse toResponse(this Account account)
    {
        return new AccountResponse
        {
            IBAN = account.IBAN,
            clientID = account.ClientId,
            productID = account.ProductId,
            AccountType = account.AccountType,
            InterestRate = account.InterestRate
        };
    }
}