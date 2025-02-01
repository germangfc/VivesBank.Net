using System.Reactive.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
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
    public async Task GetMyAccountsAsClientAsync_ShouldReturnOk_WhenAccountsExist()
    {
        var accountResponses = new List<AccountResponse>
        {
            new AccountResponse { Id = "1", IBAN = "IBAN1", AccountType = AccountType.STANDARD },
            new AccountResponse { Id = "2", IBAN = "IBAN2", AccountType = AccountType.SAVING }
        };

        _mockAccountsService.Setup(service => service.GetMyAccountsAsClientAsync())
            .ReturnsAsync(accountResponses);

        var result = await _accountController.GetMyAccountsAsClientAsync();

        ClassicAssert.IsNotNull(result);
        var okResult = result.Result as OkObjectResult; 
        ClassicAssert.IsNotNull(okResult);

        ClassicAssert.AreEqual(200, okResult.StatusCode);

        var accounts = okResult.Value as List<AccountResponse>;
        ClassicAssert.IsNotNull(accounts);
        ClassicAssert.AreEqual(2, accounts.Count); 
        ClassicAssert.AreEqual("IBAN1", accounts[0].IBAN);
        ClassicAssert.AreEqual("IBAN2", accounts[1].IBAN);

        _mockAccountsService.Verify(service => service.GetMyAccountsAsClientAsync(), Times.Once);
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
        
        _mockAccountsService.Setup(service => service.CreateAccountAsync(request)).ReturnsAsync(response);
        var result = await _accountController.CreateAccount(request);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(response));
        
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
    public async Task DeleteMyAccountAsClientAsync_ShouldReturnNoContent_WhenAccountDeleted()
    {
        string ibanToDelete = "IBAN123456789";
    
        _mockAccountsService.Setup(service => service.DeleteMyAccountAsync(ibanToDelete))
            .Returns(Task.CompletedTask);

        var result = await _accountController.DeleteMyAccountAsClientAsync(ibanToDelete);

        ClassicAssert.IsNotNull(result);
        var noContentResult = result as NoContentResult;
        ClassicAssert.IsNotNull(noContentResult);

        ClassicAssert.AreEqual(204, noContentResult.StatusCode);

        _mockAccountsService.Verify(service => service.DeleteMyAccountAsync(ibanToDelete), Times.Once);
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

    [Test]
    public async Task ExportAccountsToJson_ShouldReturnFile_WhenAccountsExist()
    {
        var accountResponses = new List<AccountResponse>
        {
            new AccountResponse { Id = "1", IBAN = "IBAN1" },
            new AccountResponse { Id = "2", IBAN = "IBAN2" }
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
            Last = true
        };

        _mockAccountsService.Setup(x => x.GetAccountsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pageResponse);

        var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "accounts.json");
        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            var writer = new StreamWriter(fileStream);
            writer.Write("[{\"Id\":\"1\",\"IBAN\":\"IBAN1\"},{\"Id\":\"2\",\"IBAN\":\"IBAN2\"}]");
            writer.Flush();
            fileStream.Position = 0;

            _mockAccountsService.Setup(x => x.Export(It.IsAny<List<Account>>()))
                .ReturnsAsync(fileStream);

            var result = await _accountController.ExportAccountsToJson();

            ClassicAssert.IsNotNull(result);
            var fileResult = result as FileStreamResult;
            ClassicAssert.IsNotNull(fileResult);
            ClassicAssert.AreEqual("application/json", fileResult.ContentType);
            ClassicAssert.AreEqual("accounts.json", fileResult.FileDownloadName);
            ClassicAssert.AreEqual(fileStream, fileResult.FileStream);
        }
    }

    

    

    [Test]
    public async Task ExportAccountsToJson_ShouldReturnNoContent_WhenNoAccountsExist()
    {
        _mockAccountsService.Setup(x => x.GetAccountsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new PageResponse<AccountResponse>
            {
                Content = new List<AccountResponse>(),
                TotalPages = 0,
                TotalElements = 0,
                PageSize = 0,
                PageNumber = 0,
                TotalPageElements = 0,
                Empty = true,
                First = true,
                Last = true
            });

        var result = await _accountController.ExportAccountsToJson();

        var noContentResult = result as NoContentResult;
        ClassicAssert.IsNotNull(noContentResult);
        ClassicAssert.AreEqual(204, noContentResult.StatusCode);
    }
}
