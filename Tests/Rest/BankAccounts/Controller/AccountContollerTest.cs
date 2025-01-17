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
        // Crear mocks para las dependencias
        _mockAccountsService = new Mock<IAccountsService>();
        _mockLogger = new Mock<ILogger<AccountController>>();

        // Crear una instancia del controlador con dependencias simuladas
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
        _mockAccountsService.Setup(service => service.GetAccountByIdAsync(It.Is<string>(id => id == account.Id)))
            .ThrowsAsync(new AccountsExceptions.AccountNotFoundException(account.Id));  // Lanzamos la excepción para simular una cuenta no encontrada

        // Act
        var result = await _accountController.GetAccountById(account.Id);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);  // Verifica que el resultado es NotFound
        Assert.That(notFoundResult.StatusCode, Is.EqualTo(404));  // Verifica que el código de estado es 404
        Assert.That(notFoundResult.Value, Is.Not.Null);
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
}