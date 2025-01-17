using Microsoft.Extensions.Logging;
using Moq;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.Base.Models;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Utils.IbanGenerator;

namespace Tests.Rest.BankAccounts.Service;
[TestFixture]
[TestOf(typeof(AccountService))]
public class AccountServiceTest
{
    [SetUp]
    public void Setup()
    {
        _accountRepository = new Mock<IAccountsRepository>();
        _clientRepository = new Mock<IClientRepository>();
        _productRepository = new Mock<IProductRepository>();
        _ibanGenerator = new Mock<IIbanGenerator>();
        _logger = new Mock<ILogger<AccountService>>();
        
        _accountService = new AccountService(_logger.Object, _ibanGenerator.Object, _clientRepository.Object, _productRepository.Object, _accountRepository.Object);

    }
    private Mock<IAccountsRepository> _accountRepository;
    private Mock<IClientRepository> _clientRepository;
    private Mock<IProductRepository> _productRepository;
    private Mock<IIbanGenerator> _ibanGenerator;
    private Mock<ILogger<AccountService>> _logger;
    private AccountService _accountService;
    
    private readonly Account account = new Account
    {
        Id = "TkjPO5u_2w",
        ClientId = "Q5hsVJ2-oQ",
        ProductId = "xFtC3Mv_oA",
        AccountType = AccountType.STANDARD,
        IBAN = "ES9121000418450200051332",
        Balance = 1000
    };

    private readonly AccountResponse _response = new AccountResponse
    {
        AccountType = AccountType.STANDARD,
        IBAN = "ES9121000418450200051332",
    };

    [Test]
    public async Task GetAccountByIdAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == account.Id)))
            .ReturnsAsync(account);
        var result = await _accountService.GetAccountByIdAsync(account.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IBAN, Is.EqualTo(_response.IBAN));
    }

    [Test]
    public async Task GetAccountByIdAsync_ShouldReturnNotFound()
    {
        var id = "notFound";
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == account.Id)))
            .ReturnsAsync((Account)null);
        var result =Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundException>(async () =>
            await _accountService.GetAccountByIdAsync(id));
        Assert.That(result.Message, Is.EqualTo($"Account not found by id {id}"));
    }

    [Test]
    public async Task getAccountByClientIdAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.getAccountByClientIdAsync(It.Is<string>(id => id == account.ClientId)))
           .ReturnsAsync(new List<Account> { account });
        var result = await _accountService.GetAccountByClientIdAsync(account.ClientId);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().IBAN, Is.EqualTo(_response.IBAN));
    }
    
    [Test]
    public void GetAccountByClientIdAsync_ShouldThrowAccountNotFoundException_WhenNoAccountsFound()
    {
        var id = "NotFound";
        _accountRepository.Setup(r => r.getAccountByClientIdAsync(It.IsAny<string>()))
            .ReturnsAsync((List<Account>)null);
        
        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundException>(async () =>
            await _accountService.GetAccountByClientIdAsync(id));

        Assert.That(exception.Message, Is.EqualTo($"Account not found by id {id}"));
    }

    [Test]
    public async Task GetAccountByIbanAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == account.IBAN)))
           .ReturnsAsync(account);
        var result = await _accountService.GetAccountByIbanAsync(account.IBAN);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IBAN, Is.EqualTo(_response.IBAN));
    }

    [Test]
    public void GetAccountByIbanAsync_ShouldThrowAccountNotFoundException_WhenNoAccountsFound()
    {
        var iban = "notFound";
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == iban)))
           .ReturnsAsync((Account)null);

        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () =>
            await _accountService.GetAccountByIbanAsync(iban));

        Assert.That(exception.Message, Is.EqualTo($"Account not found by IBAN {iban}"));
    }

    [Test]
    public async Task CreateAccountAsync_ShouldCreateSuccesfull()
    {
        var request = new CreateAccountRequest
        {
            ClientId = "TkjPO5u_2w",
            ProductName = "ValidProductName",
            AccountType = AccountType.STANDARD
        };
        var generatedIban = "ES9121000418450200051332";
        var product = new Product("ValidProductName", Product.Type.BankAccount);
        _clientRepository.Setup(r => r.GetByIdAsync(request.ClientId))
            .ReturnsAsync(new Client { Id = request.ClientId });

        _productRepository.Setup(r => r.GetByNameAsync(request.ProductName))
            .ReturnsAsync(product);
        _ibanGenerator.Setup(g => g.GenerateUniqueIbanAsync())
            .ReturnsAsync(generatedIban);
        _accountRepository.Setup(r => r.AddAsync(It.Is<Account>(a => a.ClientId == request.ClientId && a.ProductId == product.Id && a.IBAN == generatedIban)))
           .Returns(Task.CompletedTask);
        var result = await _accountService.CreateAccountAsync(request);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IBAN, Is.EqualTo(generatedIban));
    }

    [Test]
    public void CreateAccountAsync_ShouldThrowClientNotFound()
    {
        var request = new CreateAccountRequest
        {
            ClientId = "TkjPO5u_2w",
            ProductName = "",
            AccountType = AccountType.STANDARD
        };
        _clientRepository.Setup(r => r.GetByIdAsync(request.ClientId))
            .ReturnsAsync((Client)null);
        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotCreatedException>(async () =>
            await _accountService.CreateAccountAsync(request));
        Assert.That(exception.Message, Is.EqualTo("Account couldnt be created, check that te client and the product exists"));
    }

    [Test]
    public void CreateAccountAsync_ShouldThrowProductNotFound()
    {
        var request = new CreateAccountRequest
        {
            ClientId = "TkjPO5u_2w",
            ProductName = "NotExistingProduct",
            AccountType = AccountType.STANDARD
        };
        _clientRepository.Setup(r => r.GetByIdAsync(request.ClientId))
           .ReturnsAsync(new Client { Id = request.ClientId });
        _productRepository.Setup(r => r.GetByNameAsync(request.ProductName))
           .ReturnsAsync((Product)null);
        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotCreatedException>(async () =>
            await _accountService.CreateAccountAsync(request));
        Assert.That(exception.Message, Is.EqualTo("Account couldnt be created, check that te client and the product exists"));
    }

    [Test]
    public async Task DeleteAccountAsync_ShouldDeleteLogically()
    {
        var accountId = "TkjPO5u_2w";
        _accountRepository.Setup(r => r.GetByIdAsync(account.Id)).
            ReturnsAsync(account);
        _accountRepository.Setup(r => r.UpdateAsync(It.IsAny<Account>())).Callback<Account>(updatedAccount =>
        {
            Assert.That(updatedAccount.IsDeleted, Is.True);
            Assert.That(updatedAccount.Id, Is.EqualTo(account.Id));
        });
        await _accountService.DeleteAccountAsync(accountId);
        _accountRepository.Verify(r => r.GetByIdAsync(accountId), Times.Once);
        _accountRepository.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.IsDeleted == true)), Times.Once);
    }
    
}
