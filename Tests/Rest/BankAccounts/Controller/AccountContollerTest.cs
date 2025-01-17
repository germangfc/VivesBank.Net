using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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
    public async Task GetAccountById_ShouldReturnAccount_WhenAccountExists()
    {
        _mockAccountsService.Setup(service => service.GetAccountByIdAsync(It.Is<string>(id => id == account.Id)))
            .ReturnsAsync(_expectedAccountResponse);
        
        var result = await _accountController.GetAccountById(account.Id);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(_expectedAccountResponse));
    }

    [Test]
    public async Task GetAccountById_ShouldReturnNotFound()
    {
        _mockAccountsService
            .Setup(service => service.GetAccountByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new AccountsExceptions.AccountNotFoundException("TkjPO5u_2w"));
        
        var result = await _accountController.GetAccountById("TkjPO5u_2w");

        
        var notFoundResult = result.Result as NotFoundResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task GetAccountByIban_ShouldReturnAccountByIban()
    {
        _mockAccountsService.Setup(service => service.GetAccountByIbanAsync(It.Is<string>(iban => iban == account.IBAN)))
           .ReturnsAsync(_expectedAccountResponse);
        
        var result = await _accountController.GetAccountByIban(account.IBAN);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(_expectedAccountResponse));
    }

    [Test]
    public async Task GetAccountByIban_ShouldReturnNotFound()
    {
        var id = "notfound";
        _mockAccountsService.Setup(service => service.GetAccountByIbanAsync(It.IsAny<string>()))
            .ThrowsAsync(new AccountsExceptions.AccountNotFoundByIban(id));
        
        var result = await _accountController.GetAccountByIban(id);
        
        var notFoundResult = result.Result as NotFoundResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
    }

    [Test]
    public async Task CreateAccount_ShouldCreateAccount()
    {
        var request = new CreateAccountRequest
        {
            ClientId = "Q5hsVJ2-oQ",
            ProductName = "guay",
            AccountType = AccountType.STANDARD
        };
        var response = new AccountResponse
        {
            AccountType = AccountType.STANDARD,
            IBAN = "ES9121000418450200051332"
        };
        
        _mockAccountsService.Setup(service => service.CreateAccountAsync(It.Is<CreateAccountRequest>(r =>r.ClientId == request.ClientId)))
           .ReturnsAsync(response);
        
        var result = await _accountController.CreateAccount(request);
        
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.StatusCode, Is.EqualTo(200));
        Assert.That(okResult.Value, Is.EqualTo(response));
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
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));
    }
    
    
}