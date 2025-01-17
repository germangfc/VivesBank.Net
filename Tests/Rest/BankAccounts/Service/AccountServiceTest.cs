using Microsoft.Extensions.Logging;
using Moq;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Services;
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
        _ibanGenerator = new Mock<IbanGenerator>();
        _logger = new Mock<ILogger<AccountService>>();
        
        _accountService = new AccountService(_logger.Object, _ibanGenerator.Object, _clientRepository.Object, _productRepository.Object, _accountRepository.Object);

    }
    private Mock<IAccountsRepository> _accountRepository;
    private Mock<IClientRepository> _clientRepository;
    private Mock<IProductRepository> _productRepository;
    private Mock<IbanGenerator> _ibanGenerator;
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
    /*
    [Test]
    public async Task getAllAccountAsync()
    {
        _accountRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new List<Account> { account });
        var result = await _accountService.GetAccountsAsync();
        Assert.Multiple(() =>
        {
            Assert.That(result.Value[0], Is.EqualTo(account));
        });
    }
    */
}