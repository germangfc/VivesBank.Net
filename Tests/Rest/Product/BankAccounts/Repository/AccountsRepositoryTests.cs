using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Testcontainers.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;

namespace Tests.Rest.BankAccounts.Repository;
[TestFixture]
[TestOf(typeof(AccountsRepository))]
public class AccountsRepositoryTests
{
    private PostgreSqlContainer _container;
    private BancoDbContext _context;
    private AccountsRepository _repository;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
           .WithImage("postgres:15-alpine")
           .WithDatabase("testdb")
           .WithUsername("testuser")
           .WithPassword("testpassword")
           .WithPortBinding(5432, true)
           .Build();
        
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<BancoDbContext>()
           .UseNpgsql(_container.GetConnectionString())
           .Options;

        _context = new BancoDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new AccountsRepository(_context, NullLogger<AccountsRepository>.Instance);
    }
    [TearDown]
    public async Task TearDown()
    {
        await _repository.DeleteAllAsync();
    }
    
    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_context!= null)
        {
            await _context.DisposeAsync();
        }

        if (_container!= null)
        {
            await _container.StopAsync();
            await _container.DisposeAsync();
        }
    }
    private readonly Account account = new Account
    {
        Id = "TkjPO5u_2w",
        ClientId = "Q5hsVJ2-oQ",
        ProductId = "xFtC3Mv_oA",
        AccountType = AccountType.STANDARD,
        IBAN = "ES9121000418450200051332",
        Balance = 1000
    };

    [Test]
    public async Task GetAccountByIbanAsync_ReturnsAccount()
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var result = await _repository.getAccountByIbanAsync(account.IBAN);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ClientId, Is.EqualTo(account.ClientId));
    }

    [Test]
    public async Task GetAccountByIbanAsync_ReturnsNull_WhenNoAccountFound()
    {
        var iban = "ES9121000418450200051333";
        var result = await _repository.getAccountByIbanAsync(iban);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAccountByClientId_ShouldReturnClient()
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var result = await _repository.getAccountByClientIdAsync(account.ClientId);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.First().ClientId, Is.EqualTo(account.ClientId));
    }

    [Test]
    public async Task GetAccountByClientId_ShouldReturnEmptyList_WhenNoAccountFound()
    {
        var clientId = "NotARealClientId";
        var result = await _repository.getAccountByClientIdAsync(clientId);
        Assert.That(result, Is.Empty);
    }
    
    
}