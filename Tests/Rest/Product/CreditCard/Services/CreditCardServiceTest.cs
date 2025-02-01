using System.Reactive.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Users.Service;

namespace Tests.Rest.Product.CreditCard.Service;

public class CreditCardServiceTest
{
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<ICreditCardRepository> creditCardRepositoryMock;
    private Mock<ILogger<CreditCardService>> _logger;
    private Mock<IHttpContextAccessor> _contextAccessor;
    private Mock<IUserService> _userService;
    private Mock<IClientRepository> _clientRepository;
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
        _contextAccessor = new Mock<IHttpContextAccessor>();
        _userService = new Mock<IUserService>();
        _clientRepository = new Mock<IClientRepository>();
        _connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        _logger = new Mock<ILogger<CreditCardService>>();
        _connection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<string>())).Returns(_cache.Object);

        creditCardRepositoryMock = new Mock<ICreditCardRepository>();
        accountsRepositiryMock = new Mock<IAccountsRepository>();

        _cvcGenerator = new CvcGenerator();
        _expirationDateGenerator = new ExpirationDateGenerator();
        _numberGenerator = new NumberGenerator();

        CreditCardService = new CreditCardService(creditCardRepositoryMock.Object, _logger.Object, _cvcGenerator, _expirationDateGenerator, _numberGenerator, accountsRepositiryMock.Object, _connection.Object, _contextAccessor.Object, _userService.Object, _clientRepository.Object);

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
    public async Task GetCreditCardByIdAdminAsync_WhenCardFound()
    {
        // Arrange
        var cardId = _CreditCard1.Id;
        creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(_CreditCard1);

        // Act
        var result = await CreditCardService.GetCreditCardByIdAdminAsync(cardId);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result.CardNumber);
        });

        // Verify
        creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(cardId), Times.Once);
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
    public async Task CreateCreditCardAsync_WhenAccountExists()
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
    public async Task CreateCreditCardAsync_WhenAccountIsNull()
    {
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };

        accountsRepositiryMock
            .Setup(repo => repo.getAccountByIbanAsync(createRequest.AccountIban)).ReturnsAsync((Account?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<Exception>(async () =>
            await CreditCardService.CreateCreditCardAsync(createRequest));

        ClassicAssert.AreEqual(createRequest.AccountIban, exception.Message);

        // Verify
        accountsRepositiryMock.Verify(repo => repo.getAccountByIbanAsync(createRequest.AccountIban), Times.Once);
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
    
    [Test]
    public async Task Import_WhenFileIsValid_ReturnsCreditCards()
    {
        var mockFile = new Mock<IFormFile>();
        var mockStream = new MemoryStream();
        var writer = new StreamWriter(mockStream);
        writer.Write("[{\"Id\":\"1\",\"AccountId\":\"1\",\"CardNumber\":\"1234567890123456\",\"Pin\":\"123\",\"Cvc\":\"123\",\"ExpirationDate\":\"2028-02-01\",\"CreatedAt\":\"2022-01-01\",\"UpdatedAt\":\"2022-01-01\",\"IsDeleted\":false}]");
        writer.Flush();
        mockStream.Position = 0;

        mockFile.Setup(f => f.OpenReadStream()).Returns(mockStream);
        mockFile.Setup(f => f.Length).Returns(mockStream.Length);

        var result = CreditCardService.Import(mockFile.Object);

        var creditCardList = await result.ToList();  
        ClassicAssert.IsNotNull(creditCardList);
        ClassicAssert.AreEqual(1, creditCardList.Count);

        var creditCard = creditCardList.FirstOrDefault();
        ClassicAssert.AreEqual("1", creditCard?.Id);
        ClassicAssert.AreEqual("1", creditCard?.AccountId);
        ClassicAssert.AreEqual("1234567890123456", creditCard?.CardNumber);
        ClassicAssert.AreEqual("123", creditCard?.Pin);
        ClassicAssert.AreEqual("123", creditCard?.Cvc);
        ClassicAssert.AreEqual(DateOnly.FromDateTime(DateTime.Now.AddYears(3)), creditCard?.ExpirationDate);
    }
    
    [Test]
    public async Task ExportWhenValidListExportsToJsonFile()
    {
        var creditCard1 = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "1",
            CardNumber = "1234567890123456",
            Pin = "123",
            Cvc = "123",
            ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(3)),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        var creditCard2 = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "2",
            CardNumber = "9876543210987654",
            Pin = "321",
            Cvc = "321",
            ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(2)),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        var creditCardList = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard> { creditCard1, creditCard2 };

        var fileStream = await CreditCardService.Export(creditCardList);

        Assert.That(fileStream, Is.Not.Null);
        Assert.That(fileStream.CanRead, Is.True);

        var filePath = fileStream.Name;
        Assert.That(File.Exists(filePath), Is.True);

        var fileContent = await File.ReadAllTextAsync(filePath);
        Assert.That(fileContent, Does.Contain(creditCard1.CardNumber));
        Assert.That(fileContent, Does.Contain(creditCard2.CardNumber));

        fileStream.Close();
        File.Delete(filePath);
    }
    
    [Test]
    public async Task Export_WhenEmptyList_ThrowsArgumentException()
    {
        var emptyCreditCardList = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>();

        var ex = Assert.ThrowsAsync<ArgumentException>(
            async () => await CreditCardService.Export(emptyCreditCardList)
        );

        Assert.That(ex.Message, Is.EqualTo("Cannot export an empty list of credit cards."));
    }
}