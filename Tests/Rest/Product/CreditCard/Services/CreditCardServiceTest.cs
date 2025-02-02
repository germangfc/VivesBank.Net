using System.Reactive.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using Role = VivesBankApi.Rest.Users.Models.Role;

namespace Tests.Rest.Product.CreditCard.Service;

public class CreditCardServiceTest
{
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<ICreditCardRepository> _creditCardRepositoryMock;
    private Mock<ILogger<CreditCardService>> _logger;
    private Mock<IHttpContextAccessor> _contextAccessor;
    private Mock<IUserService> _userService;
    private Mock<IClientRepository> _clientRepository;
    private Mock<ICvcGenerator>_cvcGenerator;
    private Mock<IExpirationDateGenerator> _expirationDateGenerator;
    private Mock<INumberGenerator> _numberGenerator;
    private Mock<IAccountsRepository> _accountsRepositiryMock;
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

        _creditCardRepositoryMock = new Mock<ICreditCardRepository>();
        _accountsRepositiryMock = new Mock<IAccountsRepository>();

        _cvcGenerator = new Mock<ICvcGenerator>();
        _expirationDateGenerator = new Mock<IExpirationDateGenerator>();
        _numberGenerator = new Mock<INumberGenerator>();

        CreditCardService = new CreditCardService(_creditCardRepositoryMock.Object, _logger.Object, _cvcGenerator.Object, _expirationDateGenerator.Object, _numberGenerator.Object, _accountsRepositiryMock.Object, _connection.Object, _contextAccessor.Object, _userService.Object, _clientRepository.Object);

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
    public async Task GetAllCreditCardsPaginated_ShouldReturnCreditCards()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var fullName = "";
        var isDeleted = false;
        var direction = "asc";

        var cards = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard> {_CreditCard1, _CreditCard2};
        _creditCardRepositoryMock.Setup(repo => repo.GetAllCrediCardsPaginated(pageNumber, pageSize, fullName, isDeleted, direction))
           .ReturnsAsync(new PagedList<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>(cards, cards.Count, pageNumber, pageSize));

        // Act
        var result = await CreditCardService.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result[0].CardNumber);
            ClassicAssert.AreEqual(_CreditCard2.CardNumber, result[1].CardNumber);
        });
    }

    [Test]
    public async Task GetCreditCardsPaginated_ShouldReturn_ListaVacia()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var fullName = "";
        var isDeleted = true;
        var direction = "asc";

        _creditCardRepositoryMock.Setup(repo => repo.GetAllCrediCardsPaginated(pageNumber, pageSize, fullName, isDeleted, direction))
           .ReturnsAsync(new PagedList<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>(new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>(), 0, pageNumber, pageSize));

        // Act
        var result = await CreditCardService.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        });
    }

    [Test]
    public async Task GetMyCreditCardsAsync_ShouldReturnMyCreditCards()
    {
        // Arrange
        var account = new Account { Id = "account123" };
        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = "creditCard123" };
        var creditCardResponse = new CreditCardClientResponse { Id = "creditCard123" };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(new UserResponse { Id = "user123" });
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync(new Client { Id = "client123" });
        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync("client123")).ReturnsAsync(new List<Account> { account });
        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(account.Id)).ReturnsAsync(creditCard);
       

        var result = await CreditCardService.GetMyCreditCardsAsync();
        
        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(1, result.Count);
            ClassicAssert.AreEqual(creditCardResponse.Id, result[0].Id);
        });
    }

    [Test]
    public async Task GetMyCreditCarsAsync_UserNotFoundException()
    {
        var id = "user123";
        
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync((UserResponse?)null);
        
        var result = Assert.ThrowsAsync<UserNotFoundException>(async () =>
        {
            await CreditCardService.GetMyCreditCardsAsync();
        });
        ClassicAssert.AreEqual($"The user with id: {id} was not found", result.Message);
    }

    [Test]
    public async Task GertMYCreditCardAsync_ClientNotFoundException()
    {
        var id = "user123";
        
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(new UserResponse { Id = "user123" });
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync((Client?)null);
        
        var result = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
        {
            await CreditCardService.GetMyCreditCardsAsync();
        });
        ClassicAssert.AreEqual($"Client not found by id {id}", result.Message);
    }

    [Test]
    public async Task GetMyCreditCardAsync_ReturnEmptyList()
    {
        var id = "user123";
        var account = new Account { Id = "account123" };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(new UserResponse { Id = "user123" });
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync(new Client { Id = "client123" });
        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync("client123")).ReturnsAsync(new List<Account>(){account});
        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(account.Id)).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard?)null);
        
        var result = await CreditCardService.GetMyCreditCardsAsync();
        
        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        });
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
        _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(_CreditCard1.Id), Times.Never);
    }
    
    [Test]
    public async Task GetCreditCardByIdAdminAsync_WhenCardFound()
    {
        // Arrange
        var cardId = _CreditCard1.Id;
        _creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(cardId)).ReturnsAsync(_CreditCard1);

        // Act
        var result = await CreditCardService.GetCreditCardByIdAdminAsync(cardId);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result.CardNumber);
        });

        // Verify
        _creditCardRepositoryMock.Verify(repo => repo.GetByIdAsync(cardId), Times.Once);
    }

    [Test]
    public void GetCreditCardByIdAdminAsync_NotFound()
    {
        // Arrange
        _creditCardRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard?)null);

        // Act & Assert
        Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
        {
            await CreditCardService.GetCreditCardByIdAdminAsync("999");
        });
    }

    [Test]
    public async Task GetCreditCardByNumberAsync_ShouldReturn_CreditCard()
    {
        var cardNumber = _CreditCard1.CardNumber;
        _creditCardRepositoryMock.Setup(repo => repo.GetByCardNumber(cardNumber)).ReturnsAsync(_CreditCard1);
        
        var result = await CreditCardService.GetCreditCardByCardNumber(cardNumber);
        
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_CreditCard1.CardNumber, result.CardNumber);
        });
        
        _creditCardRepositoryMock.Verify(repo => repo.GetByCardNumber(cardNumber), Times.Once);
    }

    [Test]
    public async Task GetCreditCardByNumberAsync_ShouldReturn_NotFoundException()
    {
        var cardNumber = "1234567890";
        _creditCardRepositoryMock.Setup(repo => repo.GetByCardNumber(cardNumber)).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard?)null);

        Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(async () =>
        {
            await CreditCardService.GetCreditCardByCardNumber(cardNumber);
        });
        _creditCardRepositoryMock.Verify(repo => repo.GetByCardNumber(cardNumber), Times.Once);
    }
    

    [Test]
    public async Task CreateCreditCardAsync_WhenAccountExists()
    {
        var createRequest = new CreditCardRequest { AccountIban = "IBAN123" , Pin = "1234"};
        var account = new Account { Id = "account123", ClientId = "client123" };  
        var client = new Client { Id = "client123", UserId = "user123" };
        var user = new User { Id = "user123", Role = Role.Client};
        var userResponse = new UserResponse { Id = "user123" };
        var creditCardModel = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = "creditCard123", AccountId = "account123" };
        var creditCardResponse = new CreditCardClientResponse { Id = "creditCard123" };

        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(userResponse);
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync(client);
        _accountsRepositiryMock.Setup(x => x.getAccountByIbanAsync("IBAN123")).ReturnsAsync(account);

        _numberGenerator.Setup(x => x.GenerateCreditCardNumber()).Returns("1234567890123456");
        _expirationDateGenerator.Setup(x => x.GenerateRandomDate()).Returns(DateOnly.FromDateTime(DateTime.Now));
        _cvcGenerator.Setup(x => x.Generate()).Returns("123");

        _creditCardRepositoryMock.Setup(x => x.AddAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>())).Returns(Task.CompletedTask);
        
        var result = await CreditCardService.CreateCreditCardAsync(createRequest);
        
        ClassicAssert.AreEqual(createRequest.Pin, result.Pin);
    }

    [Test]
    public async Task CreateCreditCardAsync_WhenClientNotFound()
    {
        
        var user = new User { Id = "user123", Role = Role.Client};
        var userResponse = new UserResponse { Id = "user123" };
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(userResponse);
        _clientRepository.Setup(x => x.getByUserIdAsync(It.IsAny<string>())).ReturnsAsync((Client?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
            await CreditCardService.CreateCreditCardAsync(createRequest));

        ClassicAssert.AreEqual($"Client not found by id {user.Id}", exception.Message);

        // Verify
        _clientRepository.Verify(repo => repo.getByUserIdAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task CreateCreditCardAsync_UserNotFoundException()
    {
        var id = "123";
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, id));
        _userService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync((UserResponse?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await CreditCardService.CreateCreditCardAsync(createRequest));

        ClassicAssert.AreEqual($"The user with id: {id} was not found", exception.Message);

        // Verify
        _userService.Verify(repo => repo.GetUserByIdAsync(id), Times.Once);
    }

    [Test]
    public async Task CreateCreditCardAsync_AccountDoesNotExist()
    {
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
           .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(new UserResponse { Id = "user123" });
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync(new Client { Id = "client123" });
        _accountsRepositiryMock
           .Setup(repo => repo.getAccountByIbanAsync(createRequest.AccountIban)).ReturnsAsync((Account?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () =>
            await CreditCardService.CreateCreditCardAsync(createRequest));

        ClassicAssert.AreEqual($"Account not found by IBAN {createRequest.AccountIban}", exception.Message);

        // Verify
        _accountsRepositiryMock.Verify(repo => repo.getAccountByIbanAsync(createRequest.AccountIban), Times.Once);
    }

    [Test]
    public async Task CreateCreditCardAsync_OnAnAccountWithoutOwningIt()
    {
        var id = "user123";
        // Arrange
        var createRequest = new CreditCardRequest
        {
            AccountIban = "IBAN123456789",
            Pin = "1234"
        };
        _contextAccessor.Setup(x => x.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier))
           .Returns(new Claim(ClaimTypes.NameIdentifier, "user123"));
        _userService.Setup(x => x.GetUserByIdAsync("user123")).ReturnsAsync(new UserResponse { Id = "user123" });
        _clientRepository.Setup(x => x.getByUserIdAsync("user123")).ReturnsAsync(new Client { Id = "client123" });
        _accountsRepositiryMock.Setup(repo => repo.getAccountByIbanAsync(createRequest.AccountIban)).ReturnsAsync(new Account { ClientId = "client124" });

        // Act & Assert
        var exception = Assert.ThrowsAsync<ClientExceptions.ClientNotAllowedToAccessAccount>(async () =>
            await CreditCardService.CreateCreditCardAsync(createRequest));

        ClassicAssert.AreEqual($"The client with id {id} is not allowed to access the account with iban {createRequest.AccountIban}", exception.Message);
        
        _accountsRepositiryMock.Verify(repo => repo.getAccountByIbanAsync(createRequest.AccountIban), Times.Once);
    }
    
    [Test]
    public async Task UpdateCreditCardAsync_UpdatesSuccessfully()
    {
        
        var cardNumber = "1234567890123456";
        var updateRequest = new CreditCardUpdateRequest { Pin = "1234" };
        var userResponse = new UserResponse
        {
            Id = "user123",
        };

        var userId = "user123";
        var clientId = "client123";
        var accountId = "account123";

        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            CardNumber = cardNumber,
            AccountId = accountId,
            Pin = "0000"
        };

        var myCreditCards = new List<CreditCardClientResponse>
        {
            new CreditCardClientResponse { AccountId = accountId }
        };

        var user = new User { Id = userId };
        var client = new Client { Id = clientId, UserId = userId };
        var account = new Account { Id = accountId, ClientId = clientId };
        
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _contextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        _userService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        _clientRepository.Setup(x => x.getByUserIdAsync(userId))
            .ReturnsAsync(client);

        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync(clientId))
            .ReturnsAsync(new List<Account> { account });

        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(accountId))
            .ReturnsAsync(creditCard);

        _creditCardRepositoryMock.Setup(x => x.GetByCardNumber(cardNumber))
            .ReturnsAsync(creditCard);

        _creditCardRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()))
            .Returns(Task.CompletedTask);

        _cache.Setup(x => x.KeyDeleteAsync(cardNumber, CommandFlags.None))
            .ReturnsAsync(true);
        
        var result = await CreditCardService.UpdateCreditCardAsync(cardNumber, updateRequest);
        
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(updateRequest.Pin, creditCard.Pin);
        _creditCardRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>()), Times.Once);
    }
    
    [Test]
    public void UpdateCreditCardAsync_CreditCardNotFound()
    {
        var cardNumber = "1234567890123456";
        var updateRequest = new CreditCardUpdateRequest { Pin = "1234" };
        var userResponse = new UserResponse
        {
            Id = "user123",
        };

        var userId = "user123";
        var clientId = "client123";
        var accountId = "account123";

        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            CardNumber = cardNumber,
            AccountId = accountId,
            Pin = "0000"
        };

        var myCreditCards = new List<CreditCardClientResponse>
        {
            new CreditCardClientResponse { AccountId = accountId }
        };

        var user = new User { Id = userId };
        var client = new Client { Id = clientId, UserId = userId };
        var account = new Account { Id = accountId, ClientId = clientId };
        
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        _contextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        _userService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(userResponse);

        _clientRepository.Setup(x => x.getByUserIdAsync(userId))
            .ReturnsAsync(client);

        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync(clientId))
            .ReturnsAsync(new List<Account> { account });

        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(accountId))
            .ReturnsAsync(creditCard);

        _creditCardRepositoryMock.Setup(x => x.GetByCardNumber(cardNumber))
            .ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard)null);


        // Act & Assert
        var ex = Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundByCardNumberException>(() =>
            CreditCardService.UpdateCreditCardAsync(cardNumber, updateRequest));

        ClassicAssert.AreEqual($"The credit card with card number {cardNumber} was not found", ex.Message);
    }

    [Test]
    public async Task DeleteCreditCardAsync_Successfully()
    {
        var cardNumber = "123456789";
        var userId = "user-123";
        var clientId = "client-123";
        var accountId = "account-123";
        var creditCardId = "card-123";

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _contextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new User { Id = userId };
        var userResponse = new UserResponse
        {
            Id = userId,
        };
        var client = new Client { Id = clientId };
        var accounts = new List<Account> { new Account { Id = accountId } };
        var creditCards = new List<CreditCardClientResponse>
        {
            new CreditCardClientResponse { CardNumber = cardNumber, Id = creditCardId }
        };

        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = creditCardId, CardNumber = cardNumber, IsDeleted = false };

        _userService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(userResponse);
        _clientRepository.Setup(x => x.getByUserIdAsync(userId)).ReturnsAsync(client);
        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync(clientId)).ReturnsAsync(accounts);
        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(accountId)).ReturnsAsync( creditCard);
        _creditCardRepositoryMock.Setup(x => x.GetByCardNumber(cardNumber)).ReturnsAsync(creditCard);
        _cache.Setup(x => x.KeyDeleteAsync(creditCardId, default)).ReturnsAsync(true);

        // Act
        await CreditCardService.DeleteCreditCardAsync(cardNumber);

        // Assert
        _cache.Verify(x => x.KeyDeleteAsync(creditCardId, default), Times.Once);
        _creditCardRepositoryMock.Verify(x => x.UpdateAsync(It.Is<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>(c => c.IsDeleted == true)), Times.Once);
    }

    [Test]
    public async Task DeleteCreditCardAsync_CreditCardNotFound()
    {
        var cardNumber = "123456789";
        var userId = "user-123";
        var clientId = "client-123";
        var accountId = "account-123";
        var creditCardId = "card-123";

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        _contextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var user = new User { Id = userId };
        var userResponse = new UserResponse
        {
            Id = userId,
        };
        var client = new Client { Id = clientId };
        var accounts = new List<Account> { new Account { Id = accountId } };
        var creditCards = new List<CreditCardClientResponse>
        {
            new CreditCardClientResponse { CardNumber = cardNumber, Id = creditCardId }
        };

        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard { Id = creditCardId, CardNumber = cardNumber, IsDeleted = false };

        _userService.Setup(x => x.GetUserByIdAsync(userId)).ReturnsAsync(userResponse);
        _clientRepository.Setup(x => x.getByUserIdAsync(userId)).ReturnsAsync(client);
        _accountsRepositiryMock.Setup(x => x.getAccountByClientIdAsync(clientId)).ReturnsAsync(accounts);
        _creditCardRepositoryMock.Setup(x => x.GetCardsByAccountId(accountId)).ReturnsAsync( creditCard);
        _creditCardRepositoryMock.Setup(x => x.GetByCardNumber(cardNumber)).ReturnsAsync((VivesBankApi.Rest.Product.CreditCard.Models.CreditCard)null);

         Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundByCardNumberException>(
            async () => await CreditCardService.DeleteCreditCardAsync(cardNumber));
    }
    
    [Test]
    public async Task Import_WhenFileIsValid_ReturnsCreditCards()
    {
        var mockFile = new Mock<IFormFile>();
        var mockStream = new MemoryStream();
        var writer = new StreamWriter(mockStream);
        writer.Write("[{\"Id\":\"1\",\"AccountId\":\"1\",\"CardNumber\":\"1234567890123456\",\"Pin\":\"123\",\"Cvc\":\"123\",\"ExpirationDate\":\"2028-02-02\",\"CreatedAt\":\"2022-01-01\",\"UpdatedAt\":\"2022-01-01\",\"IsDeleted\":false}]");
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