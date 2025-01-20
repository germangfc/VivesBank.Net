using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Utils.IbanGenerator;
using VivesBankApi.Rest.Product.Base.Models;

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
    public async Task GetAccountsAsync_ReturnsPagedAccounts()
    {
        var accounts = new List<Account>
        {
            new Account { Id = "1", ClientId = "C1", ProductId = "P1", AccountType = AccountType.STANDARD, IBAN = "IBAN1", Balance = 1000 },
            new Account { Id = "2", ClientId = "C2", ProductId = "P2", AccountType = AccountType.STANDARD, IBAN = "IBAN2", Balance = 2000 },
            new Account { Id = "3", ClientId = "C3", ProductId = "P3", AccountType = AccountType.STANDARD, IBAN = "IBAN3", Balance = 3000 }
        };
        var pagedList = PagedList<Account>.Create(accounts, pageNumber: 0, pageSize: 2);

        _accountRepository
            .Setup(repo => repo.GetAllPagedAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(pagedList);

        var result = await _accountService.GetAccountsAsync(pageNumber: 0, pageSize: 2, sortBy: "id", direction: "asc");
        
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(2, result.Content.Count);
        ClassicAssert.AreEqual("1", result.Content[0].Id);
        ClassicAssert.AreEqual("2", result.Content[1].Id);
        ClassicAssert.AreEqual(2, result.PageSize);
        ClassicAssert.AreEqual(0, result.PageNumber);
        ClassicAssert.AreEqual(2, result.TotalPageElements);
        ClassicAssert.AreEqual(2, result.TotalPages);
        ClassicAssert.IsFalse(result.Empty);
        ClassicAssert.IsTrue(result.First);
        ClassicAssert.IsFalse(result.Last);
        
        _accountRepository.Verify(repo => repo.GetAllPagedAsync(0, 2), Times.Once);
    }


    [Test]
    public async Task GetAccountByIdAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == account.Id)))
            .ReturnsAsync(account);
        var result = await _accountService.GetAccountByIdAsync(account.Id);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IBAN, Is.EqualTo(_response.IBAN));
        
        _accountRepository.Verify(repo => repo.GetByIdAsync(account.Id),Times.Once);
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
        
        _accountRepository.Verify(repo => repo.GetByIdAsync(id), Times.Once);
    }

    [Test]
    public async Task getAccountByClientIdAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.getAccountByClientIdAsync(It.Is<string>(id => id == account.ClientId)))
           .ReturnsAsync(new List<Account> { account });
        var result = await _accountService.GetAccountByClientIdAsync(account.ClientId);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.First().IBAN, Is.EqualTo(_response.IBAN));
        
        _accountRepository.Verify(repo => repo.getAccountByClientIdAsync(account.ClientId), Times.Once);
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
        _accountRepository.Verify(repo => repo.getAccountByClientIdAsync(id), Times.Once);
    }

    [Test]
    public async Task GetAccountByIbanAsync_ShouldReturnAccount()
    {
        _accountRepository.Setup(r => r.GetByIdAsync(It.Is<string>(id => id == account.IBAN)))
           .ReturnsAsync(account);
        var result = await _accountService.GetAccountByIbanAsync(account.IBAN);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IBAN, Is.EqualTo(_response.IBAN));
        
        _accountRepository.Verify(repo => repo.GetByIdAsync(account.IBAN), Times.Once);
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
        
        _accountRepository.Verify(repo => repo.GetByIdAsync(iban), Times.Once);
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
        var product = new VivesBankApi.Rest.Product.Base.Models.Product("ValidProductName", VivesBankApi.Rest.Product.Base.Models.Product.Type.BankAccount);
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
        
        _clientRepository.Verify(r => r.GetByIdAsync(request.ClientId), Times.Once);
        _productRepository.Verify(r => r.GetByNameAsync(request.ProductName), Times.Once);
        _ibanGenerator.Verify(g => g.GenerateUniqueIbanAsync(), Times.Once);
        _accountRepository.Verify(r => r.AddAsync(It.Is<Account>(a => a.ClientId == request.ClientId && a.ProductId == product.Id && a.IBAN == generatedIban)), Times.Once);
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
        
        _clientRepository.Verify(r => r.GetByIdAsync(request.ClientId), Times.Once);
        _productRepository.Verify(r => r.GetByNameAsync(request.ProductName), Times.Never);
        _ibanGenerator.Verify(g => g.GenerateUniqueIbanAsync(), Times.Never);
        _accountRepository.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
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
           .ReturnsAsync((VivesBankApi.Rest.Product.Base.Models.Product)null);
        var exception = Assert.ThrowsAsync<AccountsExceptions.AccountNotCreatedException>(async () =>
            await _accountService.CreateAccountAsync(request));
        Assert.That(exception.Message, Is.EqualTo("Account couldnt be created, check that te client and the product exists"));
        
        _clientRepository.Verify(r => r.GetByIdAsync(request.ClientId), Times.Once);
        _productRepository.Verify(r => r.GetByNameAsync(request.ProductName), Times.Once);
        _ibanGenerator.Verify(g => g.GenerateUniqueIbanAsync(), Times.Never);
        _accountRepository.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
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
