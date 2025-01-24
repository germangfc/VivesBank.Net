using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;

namespace VivesBankApi.Rest.Product.BankAccounts.Mappers;

public static class AccountMappers
{
    public static AccountResponse toResponse(this Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            IBAN = account.IBAN,
            clientID = account.ClientId,
            productID = account.ProductId,
            AccountType = account.AccountType,
            InterestRate = account.InterestRate
        };
    }

    public static Account fromDtoRequest(this CreateAccountRequest createRequest)
    {
        return new Account
        {
            ClientId = createRequest.ClientId,
            ProductId = createRequest.ProductName, // Luego en el servicio le cambio el nombre por el id del producto
            AccountType = createRequest.AccountType,
        };
    }
    
    public static AccountCompleteResponse toCompleteResponse(this Account account)
    {
        return new AccountCompleteResponse
        {
            Id = account.Id,
            IBAN = account.IBAN,
            clientID = account.ClientId,
            productID = account.ProductId,
            AccountType = account.AccountType,
            InterestRate = account.InterestRate,
            Balance = account.Balance,
            TarjetaId = account.TarjetaId,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            IsDeleted = account.IsDeleted
        };
    }
}