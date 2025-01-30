using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using Refit;
using Serilog;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Controller;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;

namespace Tests.Rest.BankAccounts.Controller;

[TestFixture]
public class AccountContollerTest
{
    private Mock<IAccountsService> _mockAccountsService;
    private Mock<ILogger<AccountController>> _mockLogger;
    private AccountController _accountController;
    
    private readonly Account account = new Account
    {
        Id = "TkjPO5u_2w",
        ClientId = "Q5hsVJ2-oQ",
        ProductId = "xFtC3Mv_oA",
        AccountType = AccountType.STANDARD,
        IBAN = "ES9121000418450200051332",
        Balance = 1000
    };
    
    private readonly AccountResponse _expectedAccountResponse = new AccountResponse
    {
        AccountType = AccountType.STANDARD,
        IBAN = "ES9121000418450200051332"
    };

    [SetUp]
    public void SetUp()
    {
        _mockAccountsService = new Mock<IAccountsService>();
        _mockLogger = new Mock<ILogger<AccountController>>();
        
        _accountController = new AccountController(_mockAccountsService.Object, _mockLogger.Object);
    }
    
    [Test]
    public async Task GetAllAccounts_ReturnsOkWithPageResponse()
    {
        var page = 0;
        var size = 2;
        var sortBy = "id";
        var direction = "asc";

        var accountResponses = new List<AccountResponse>
        {
            new AccountResponse { Id = "1", IBAN = "IBAN1"},
            new AccountResponse { Id = "2", IBAN = "IBAN2"}
        };

        var pageResponse = new PageResponse<AccountResponse>
        {
            Content = accountResponses,
            TotalPages = 1,
            TotalElements = 2,
            PageSize = 2,
            PageNumber = 0,
            TotalPageElements = 2,
            Empty = false,
            First = true,
            Last = true,
            SortBy = sortBy,
            Direction = direction
        };

        _mockAccountsService
            .Setup(service => service.GetAccountsAsync(page, size, sortBy, direction))
            .ReturnsAsync(pageResponse);
        
        var result = await _accountController.GetAllAccounts(page, size, sortBy, direction) as OkObjectResult;
        
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(200, result.StatusCode);

        var response = result.Value as PageResponse<AccountResponse>;
        ClassicAssert.IsNotNull(response);
        ClassicAssert.AreEqual(2, response.Content.Count);
        ClassicAssert.AreEqual("1", response.Content[0].Id);
        ClassicAssert.AreEqual("2", response.Content[1].Id);
        ClassicAssert.AreEqual(2, response.PageSize);
        ClassicAssert.AreEqual(0, response.PageNumber);
        ClassicAssert.IsFalse(response.Empty);
        ClassicAssert.IsTrue(response.First);
        ClassicAssert.IsTrue(response.Last);
        
        _mockAccountsService.Verify(service => service.GetAccountsAsync(page, size, sortBy, direction), Times.Once);
    }

    [Test]
    public async Task GetAccountById_ShouldReturnAccount_WhenAccountExists()
    {
        _mockAccountsService.Setup(service => service.GetAccountByIdAsync(It.Is<string>(id => id == account.Id)))
            .ReturnsAsync(_expectedAccountResponse);
        
        var result = await _accountController.GetAccountById(account.Id);
        
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(_expectedAccountResponse, okResult.Value);
        
        _mockAccountsService.Verify(service => service.GetAccountByIdAsync(account.Id), Times.Once);
    }

    [Test]
    public async Task GetAccountById_ShouldReturnNotFound()
    {
        _mockAccountsService
            .Setup(service => service.GetAccountByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new AccountsExceptions.AccountNotFoundException("TkjPO5u_2w"));
        
        var result = await _accountController.GetAccountById("TkjPO5u_2w");

        var notFoundResult = result.Result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode);
        
        _mockAccountsService.Verify(service => service.GetAccountByIdAsync(account.Id), Times.Once);
    }

    [Test]
    public async Task GetAccountByIban_ShouldReturnAccountByIban()
    {
        _mockAccountsService.Setup(service => service.GetAccountByIbanAsync(It.Is<string>(iban => iban == account.IBAN)))
           .ReturnsAsync(_expectedAccountResponse);
        
        var result = await _accountController.GetAccountByIban(account.IBAN);
        
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(_expectedAccountResponse, okResult.Value);
        
        _mockAccountsService.Verify(service => service.GetAccountByIbanAsync(account.IBAN), Times.Once);
    }

    [Test]
    public async Task GetAccountByIban_ShouldReturnNotFound()
    {
        var id = "notfound";
        _mockAccountsService.Setup(service => service.GetAccountByIbanAsync(It.IsAny<string>()))
            .ThrowsAsync(new AccountsExceptions.AccountNotFoundByIban(id));
        
        var result = await _accountController.GetAccountByIban(id);
        
        var notFoundResult = result.Result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode);
    }

    [Test]
    public async Task CreateAccount_ShouldCreateAccount()
    {
        var request = new CreateAccountRequest
        {
            ProductName = "guay",
            AccountType = AccountType.STANDARD
        };
        var response = new AccountResponse
        {
            AccountType = AccountType.STANDARD,
            IBAN = "ES9121000418450200051332"
        };
        
        var result = await _accountController.CreateAccount(request);
        
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.AreEqual(response, okResult.Value);
        
        _mockAccountsService.Verify(service => service.CreateAccountAsync(request), Times.Once);
    }

    [Test]
    public async Task DeleteById_Should_Logically_Delete()
    {
        _mockAccountsService.Setup(service => service.DeleteAccountAsync(It.Is<string>(id => id == account.Id))).Returns(Task.CompletedTask);
        
        await _accountController.DeleteAccount(account.Id);
        
        _mockAccountsService.Verify(service => service.DeleteAccountAsync(account.Id), Times.Once);
    }

    [Test]
    public async Task DeleteById_Should_Return_NotFound()
    {
        _mockAccountsService.Setup(service => service.DeleteAccountAsync(It.IsAny<string>()))
           .ThrowsAsync(new AccountsExceptions.AccountNotFoundException("TkjPO5u_2w"));
        
        var result = await _accountController.DeleteAccount("TkjPO5u_2w");
        
        var notFoundResult = result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode);
        
        _mockAccountsService.Verify(service => service.DeleteAccountAsync("TkjPO5u_2w"), Times.Once);
    }

    [Test]
    public async Task ImportAccountsFromJsonWhenFileIsNull()
    {
        IFormFile file = null;

        var result = await _accountController.ImportAccountsFromJson(file);

        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("No file uploaded.", badRequestResult.Value);
    }

    [Test]
    public async Task ImportAccountsFromJsonOk()
    {
        var mockFile = new Mock<IFormFile>();
        var mockStream = new MemoryStream();
        var writer = new StreamWriter(mockStream);
        writer.Write("[{\"Id\":\"1\",\"ClientId\":\"Client1\",\"ProductId\":\"Product1\",\"AccountType\":\"STANDARD\",\"IBAN\":\"IBAN1\",\"Balance\":1000}]");
        writer.Flush();
        mockStream.Position = 0;

        mockFile.Setup(f => f.OpenReadStream()).Returns(mockStream);
        mockFile.Setup(f => f.Length).Returns(mockStream.Length);

        var accountsList = new List<Account>
        {
            new Account { Id = "1", ClientId = "Client1", ProductId = "Product1", AccountType = AccountType.STANDARD, IBAN = "IBAN1", Balance = 1000 }
        };

        _mockAccountsService.Setup(service => service.Import(It.IsAny<IFormFile>()))
            .Returns(Observable.Create<Account>(observer =>
            {
                foreach (var account in accountsList)
                {
                    observer.OnNext(account);
                }
                observer.OnCompleted();
                return () => { };
            }));

        var result = await _accountController.ImportAccountsFromJson(mockFile.Object);

        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);  
        var importedAccounts = okResult?.Value as List<Account>;
        ClassicAssert.IsNotNull(importedAccounts);  
        ClassicAssert.AreEqual(1, importedAccounts?.Count);  
    }

    [Test]
    public async Task ImportAccountsFromJsonWhenFileIsEmpty()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0); 

        var result = await _accountController.ImportAccountsFromJson(mockFile.Object);

        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("No file uploaded.", badRequestResult?.Value); 
    }
}
