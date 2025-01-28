﻿using VivesBankApi.Rest.Product.BankAccounts.Dto;
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
            ProductId = createRequest.ProductName, 
            AccountType = createRequest.AccountType,
        };
    }
    
    public static Account fromDtoRequest(this UpdateAccountRequest updateRequest)
    {
        return new Account
        {
            ProductId = updateRequest.ProductID, // Luego en el servicio le cambio el nombre por el id del producto
            ClientId = updateRequest.ClientID,
            TarjetaId = updateRequest.TarjetaId,
            IBAN = updateRequest.IBAN,
            Balance = updateRequest.Balance,
            AccountType = updateRequest.AccountType
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