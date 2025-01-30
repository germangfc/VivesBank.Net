using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

namespace Tests.Rest.Product.BankAccounts.Mappers;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;

[TestFixture]
public class AccountMappersTests
{
    [Test]
    public void ToResponse_MapsAccountToAccountResponse_Correctly()
    {
        // Arrange
        var account = new Account
        {
            Id = "123",
            IBAN = "BE71096123456769",
            ClientId = "Client-456",
            ProductId = "Product-789",
            AccountType = AccountType.SAVING
        };

        // Act
        var response = account.toResponse();

        // Assert
        ClassicAssert.IsNotNull(response);
        ClassicAssert.AreEqual(account.Id, response.Id);
        ClassicAssert.AreEqual(account.IBAN, response.IBAN);
        ClassicAssert.AreEqual(account.ClientId, response.clientID);
        ClassicAssert.AreEqual(account.ProductId, response.productID);
        ClassicAssert.AreEqual(account.AccountType, response.AccountType);
        ClassicAssert.AreEqual(account.InterestRate, response.InterestRate);
    }

    [Test]
    public void FromDtoRequest_MapsCreateAccountRequestToAccount_Correctly()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            ProductName = "Product-001",
            AccountType = AccountType.STANDARD
        };

        // Act
        var account = request.fromDtoRequest();

        // Assert
        ClassicAssert.IsNotNull(account);
        ClassicAssert.AreEqual(request.ProductName, account.ProductId);
        ClassicAssert.AreEqual(request.AccountType, account.AccountType);
    }

    [Test]
    public void FromDtoRequest_MapsUpdateAccountRequestToAccount_Correctly()
    {
        // Arrange
        var request = new UpdateAccountRequest
        {
            ProductID = "Product-002",
            ClientID = "Client-789",
            TarjetaId = "Card-123",
            IBAN = "BE71096123456770",
            Balance = 2500,
            AccountType = AccountType.STANDARD
        };

        // Act
        var account = request.fromDtoRequest();

        // Assert
        ClassicAssert.IsNotNull(account);
        ClassicAssert.AreEqual(request.ProductID, account.ProductId);
        ClassicAssert.AreEqual(request.ClientID, account.ClientId);
        ClassicAssert.AreEqual(request.TarjetaId, account.TarjetaId);
        ClassicAssert.AreEqual(request.IBAN, account.IBAN);
        ClassicAssert.AreEqual(request.Balance, account.Balance);
        ClassicAssert.AreEqual(request.AccountType, account.AccountType);
    }

    [Test]
    public void ToCompleteResponse_MapsAccountToAccountCompleteResponse_Correctly()
    {
        // Arrange
        var account = new Account
        {
            Id = "456",
            IBAN = "BE71096123456771",
            ClientId = "Client-001",
            ProductId = "Product-003",
            AccountType = AccountType.SAVING,
            Balance = 1500,
            TarjetaId = "Card-789",
            CreatedAt = new DateTime(2024, 1, 1, 10, 30, 0),
            UpdatedAt = new DateTime(2024, 1, 2, 12, 45, 0),
            IsDeleted = false
        };

        // Act
        var response = account.toCompleteResponse();

        // Assert
        ClassicAssert.IsNotNull(response);
        ClassicAssert.AreEqual(account.Id, response.Id);
        ClassicAssert.AreEqual(account.IBAN, response.IBAN);
        ClassicAssert.AreEqual(account.ClientId, response.clientID);
        ClassicAssert.AreEqual(account.ProductId, response.productID);
        ClassicAssert.AreEqual(account.AccountType, response.AccountType);
        ClassicAssert.AreEqual(account.InterestRate, response.InterestRate);
        ClassicAssert.AreEqual(account.Balance, response.Balance);
        ClassicAssert.AreEqual(account.TarjetaId, response.TarjetaId);
        ClassicAssert.AreEqual(account.CreatedAt, response.CreatedAt);
        ClassicAssert.AreEqual(account.UpdatedAt, response.UpdatedAt);
        ClassicAssert.AreEqual(account.IsDeleted, response.IsDeleted);
    }
}
