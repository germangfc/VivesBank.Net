using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;
using VivesBankApi.Rest.Product.CreditCard.Service;

namespace Tests.Rest.Product.CreditCard.Service;

public class CreditCardServiceTest
{
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<ICreditCardRepository> creditCardRepositoryMock;
    private Mock<ILogger<CreditCardService>> _logger;
    private CvcGenerator _cvcGenerator;
    private ExpirationDateGenerator _expirationDateGenerator;
    private NumberGenerator _numberGenerator;
    private Mock<IAccountsRepository> accountsRepositiryMock;
    private Mock<IDatabase> _cache;

    private CreditCardService CreditCardService;

    private VivesBankApi.Rest.Product.CreditCard.Models.CreditCard _CreditCard1;
    private VivesBankApi.Rest.Product.CreditCard.Models.CreditCard _CreditCard2;

    [SetUp]
    public void SetUp()
    {
        _connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        _logger = new Mock<ILogger<CreditCardService>>();
        _connection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<string>())).Returns(_cache.Object);

        creditCardRepositoryMock = new Mock<ICreditCardRepository>();
        accountsRepositiryMock = new Mock<IAccountsRepository>();

        _cvcGenerator = new CvcGenerator();
        _expirationDateGenerator = new ExpirationDateGenerator();
        _numberGenerator = new NumberGenerator();

        CreditCardService = new CreditCardService(creditCardRepositoryMock.Object, _logger.Object, _cvcGenerator, _expirationDateGenerator, _numberGenerator, accountsRepositiryMock.Object, _connection.Object);

        _CreditCard1 = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "1",
            AccountId = "1",
            CardNumber = "1234567890123456",
            Pin = "123",
            Cvc = "123",
            ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(3)),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        _CreditCard2 = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "2",
            AccountId = "2",
            CardNumber = "1234567899876543",
            Pin = "123",
            Cvc = "123",
            ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(3)),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };
    }
    
    [Test]
    public async Task GetAllCreditCardAdminAsync_ShouldReturnAllCreditCards()
    {
        // Arrange
        var creditCards = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard> { _CreditCard1, _CreditCard2 };

        creditCardRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(creditCards);

        // Act
        var result = await CreditCardService.GetAllCreditCardAdminAsync();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result[0].CardNumber);
            ClassicAssert.AreEqual(_CreditCard2.CardNumber, result[1].CardNumber);
        });

        // Verify
        creditCardRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetCreditCardByIdAsync_WhenInCache()
    {
        // Arrange
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonSerializer.Serialize(_CreditCard1));

        // Act
        var result = await CreditCardService.GetCreditCardByIdAdminAsync(_CreditCard1.Id);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result.CardNumber);
        });

        // Verify
        creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(_CreditCard1.Id), Times.Never);
    }

    [Test]
    public void GetCreditCardByIdAdminAsync_NotFound()
    {
        // Arrange
        creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard?)null);

        // Act & Assert
        Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
        {
            await CreditCardService.GetCreditCardByIdAdminAsync("999");
        });
    }
    
    

    [Test]
    public async Task CreateCreditCardAsync_WhenAccountExists_ShouldCreateCreditCard()
    {
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };

        var account = new Account
        {
            Id = "1",
            IBAN = "IBAN123456789"
        };

        accountsRepositiryMock
            .Setup(repo => repo.getAccountByIbanAsync(createRequest.AccountIban))
            .ReturnsAsync(account);

        creditCardRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()))
            .Returns(Task.CompletedTask);

        // Act
        var response = await CreditCardService.CreateCreditCardAsync(createRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(response);
            ClassicAssert.AreEqual(account.Id, response.AccountId);
        });

        creditCardRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()), Times.Once);
    }

    [Test]
    public async Task UpdateCreditCardAsync()
    {
        // Arrange
        var creditCardId = _CreditCard1.Id;
        var creditCardUpdateRequest = new CreditCardUpdateRequest
        {
            Pin = "123"
        };

        var existingCreditCard = _CreditCard1;

        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonSerializer.Serialize(existingCreditCard));
        creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(creditCardId)).ReturnsAsync(existingCreditCard);
        creditCardRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>())).Returns(Task.CompletedTask);

        // Act
        var result = await CreditCardService.UpdateCreditCardAsync(creditCardId, creditCardUpdateRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(existingCreditCard.CardNumber, result.CardNumber);
            ClassicAssert.AreEqual(existingCreditCard.AccountId, result.AccountId);
        });

        // Verify
        creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(creditCardId), Times.Once);
        creditCardRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()), Times.Once);
    }

    [Test]
    public void UpdateCreditCardAsync_NotFound()
    {
        // Arrange
        var creditCardId = "999";
        var creditCardUpdateRequest = new CreditCardUpdateRequest
        {
            Pin = "123"
        };

        creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(creditCardId)).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard?)null);

        // Act & Assert
        Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
        {
            await CreditCardService.UpdateCreditCardAsync(creditCardId, creditCardUpdateRequest);
        });

        // Verify
        creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(creditCardId), Times.Once);
        creditCardRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()), Times.Never);
    }

    [Test]
    public async Task DeleteCreditCardAsync()
    {
        // Arrange
        var creditCardId = _CreditCard1.Id;

        _cache.Setup(db => db.KeyDeleteAsync(creditCardId, It.IsAny<CommandFlags>())).ReturnsAsync(true);
        creditCardRepositoryMock.Setup(repo => repo.DeleteAsync(creditCardId)).Returns(Task.CompletedTask);

        // Act
        await CreditCardService.DeleteCreditCardAsync(creditCardId);

        // Assert
        _cache.Verify(db => db.KeyDeleteAsync(creditCardId, It.IsAny<CommandFlags>()), Times.Once);
        creditCardRepositoryMock.Verify(repo => repo.DeleteAsync(creditCardId), Times.Once);
    }
}