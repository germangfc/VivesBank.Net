using VivesBankApi.Products.BankAccounts.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Dto;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Products.BankAccounts.Mappers;

namespace VivesBankApi.Rest.Products.BankAccounts.Services;

public class AccountService : IAccountsService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountsRepository _accountsRepository;
    
    public AccountService(ILogger<AccountService> logger, IAccountsRepository accountsRepository)
    {
        _logger = logger;
        _accountsRepository = accountsRepository;
    }
    public async Task<List<AccountResponse>> GetAccountsAsync()
    {
        _logger.LogInformation("Getting all accounts");
        var res = await _accountsRepository.GetAllAsync();
        return res.Select(a => a.toResponse()).ToList();
    }

    public async Task<AccountResponse> GetAccountByIdAsync(string id)
    {
        _logger.LogInformation($"Getting account by id: {id}");
        var result = await _accountsRepository.GetByIdAsync(id);
        if (result == null) throw new AccountsExceptions.AccountNotFoundException(id);
        return result.toResponse();
    }

    public async Task<List<AccountResponse>> GetAccountByClientIdAsync(string clientId)
    {
        _logger.LogInformation($"Getting account by client id: {clientId}");
        var res = await _accountsRepository.getAccountByClientIdAsync(clientId);
        if (res == null) throw new AccountsExceptions.AccountNotFoundException(clientId);
        return res.Select(a => a.toResponse()).ToList();
    }

    public async Task<AccountResponse> GetAccountByIbanAsync(string iban)
    {
        _logger.LogInformation($"Getting account by IBAN {iban}");
        var result = await _accountsRepository.GetByIdAsync(iban);
        if (result == null) throw new AccountsExceptions.AccountNotFoundException(iban);
        return result.toResponse();
    }

    public Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        throw new NotImplementedException();
        
    }

    public Task DeleteAccountAsync(string id)
    {
        _logger.LogInformation($"Deleting account with ID {id}");
        return _accountsRepository.DeleteAsync(id);
    }
}