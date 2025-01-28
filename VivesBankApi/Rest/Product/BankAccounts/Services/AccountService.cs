using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Movimientos.Validators;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Utils.IbanGenerator;

namespace VivesBankApi.Rest.Product.BankAccounts.Services;

public class AccountService : IAccountsService
{
    private readonly ILogger<AccountService> _logger;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;
    private readonly IIbanGenerator _ibanGenerator;
    private readonly IDatabase _cache;
    
    public AccountService(ILogger<AccountService> logger, IIbanGenerator ibanGenerator,IClientRepository clientRepository,IProductRepository productRepository ,IAccountsRepository accountsRepository, IConnectionMultiplexer connection)
    {
        _logger = logger;
        _ibanGenerator = ibanGenerator;
        _accountsRepository = accountsRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _cache = connection.GetDatabase();
    }
    public async Task<PageResponse<AccountResponse>> GetAccountsAsync(int pageNumber = 0, int pageSize = 10, string sortBy = "id", string direction = "asc")
    {
        _logger.LogInformation("Getting all accounts with pagination");
        
        var pagedAccounts = await _accountsRepository.GetAllPagedAsync(pageNumber, pageSize);
        
        var accountResponses = pagedAccounts.Select(a => a.toResponse()).ToList();
        
        var response = new PageResponse<AccountResponse>
        {
            Content = accountResponses,
            TotalPages = pagedAccounts.PageCount,
            TotalElements = pagedAccounts.TotalCount,
            PageSize = pagedAccounts.PageSize,
            PageNumber = pagedAccounts.PageNumber,
            TotalPageElements = pagedAccounts.Count,
            Empty = pagedAccounts.Count == 0,
            First = pagedAccounts.IsFirstPage,
            Last = pagedAccounts.IsLastPage,
            SortBy = sortBy,
            Direction = direction
        };

        return response;
    }


    public Task<List<AccountResponse>> GetAccountsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AccountResponse> GetAccountByIdAsync(string id)
    {
        _logger.LogInformation($"Getting account by id: {id}");
        var result = await GetByIdAsync(id);
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
    public async Task<List<AccountCompleteResponse>> GetCompleteAccountByClientIdAsync(string clientId)
    {
        _logger.LogInformation($"Getting complete accounts by client id: {clientId}");
        var res = await _accountsRepository.getAccountByClientIdAsync(clientId);
        if (res == null) throw new AccountsExceptions.AccountNotFoundException(clientId);
        return res.Select(a => a.toCompleteResponse()).ToList();
    }

    public async Task<AccountResponse> GetAccountByIbanAsync(string iban)
    {
        _logger.LogInformation($"Getting account by IBAN {iban}");
        var result = await GetByIbanAsync(iban);
        if (result == null) throw new AccountsExceptions.AccountNotFoundByIban(iban);
        return result.toResponse();
    }
    public async Task<AccountCompleteResponse> GetCompleteAccountByIbanAsync(string iban)
    {
        _logger.LogInformation($"Getting complete account by IBAN {iban}");
        var result = await _accountsRepository.getAccountByIbanAsync(iban);
        if (result == null) throw new AccountsExceptions.AccountNotFoundByIban(iban);
        return result.toCompleteResponse();
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        _logger.LogInformation($"Creating account for Client {request.ClientId}");
        if (await _clientRepository.GetByIdAsync(request.ClientId) == null)
            throw new AccountsExceptions.AccountNotCreatedException();
        var product = await _productRepository.GetByNameAsync(request.ProductName);
        if(product == null)
            throw new AccountsExceptions.AccountNotCreatedException();
        var productId = product.Id;
        var Iban = await _ibanGenerator.GenerateUniqueIbanAsync();
        var account = request.fromDtoRequest();
        account.ProductId = productId;
        account.IBAN = Iban;
        account.Balance = 0;
        await _accountsRepository.AddAsync(account);
        return account.toResponse();
    }

    public async Task<AccountResponse> UpdateAccountAsync(string id, UpdateAccountRequest request)
    {
        _logger.LogInformation($"Updating account with id {id}");
        
        var account = await _accountsRepository.GetByIdAsync(id);
        if (account == null) throw new AccountsExceptions.AccountNotFoundException(id);
        
        if (await _productRepository.GetByNameAsync(request.ProductID) == null)
            throw new AccountsExceptions.AccountNotUpdatedException(id);
    
        if (await _clientRepository.GetByIdAsync(request.ClientID) == null)
            throw new AccountsExceptions.AccountNotCreatedException();

        if (!IbanValidator.ValidateIban(request.IBAN)) 
            throw new AccountsExceptions.AccountIbanNotValid(request.IBAN);

        if (!Enum.IsDefined(typeof(AccountType), request.AccountType)) 
            throw new AccountsExceptions.AccountNotUpdatedException(id);

        var updatingAccount = request.fromDtoRequest();
        updatingAccount.Id = account.Id;
        updatingAccount.UpdatedAt = DateTime.UtcNow;
        updatingAccount.IsDeleted = account.IsDeleted;

        await _accountsRepository.UpdateAsync(updatingAccount);
        return updatingAccount.toResponse();
        
    }

    public async Task DeleteAccountAsync(string id)
    {
        _logger.LogInformation($"Deleting account with ID {id}");
        var result = await GetByIdAsync(id);
        if (result == null) throw new AccountsExceptions.AccountNotFoundException(id);
        result.IsDeleted = true;
        await _accountsRepository.UpdateAsync(result);
        await _cache.KeyDeleteAsync(id);
        await _cache.KeyDeleteAsync("account:" + result.IBAN);
    }
    
    private async Task<Account?> GetByIdAsync(string id)
    {
        // Try to get from cache first
        var cachedAccount = await _cache.StringGetAsync(id);
        if (!cachedAccount.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Account>(cachedAccount);
        }

        // If not in cache, get from DB and cache it
        Account? account = await _accountsRepository.GetByIdAsync(id);
        if (account != null)
        {
            await _cache.StringSetAsync(id, JsonConvert.SerializeObject(account), TimeSpan.FromMinutes(10));
            return account;
        }
        return null;
    }
    
    private async Task<Account?> GetByIbanAsync(string iban)
    {
        // Try to get from cache first
        var cachedAccount = await _cache.StringGetAsync("account:" + iban);
        if (!cachedAccount.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Account>(cachedAccount);
        }

        // If not in cache, get from DB and cache it
        Account? account = await _accountsRepository.getAccountByIbanAsync(iban);
        if (account != null)
        {
            await _cache.StringSetAsync("account:" + iban, JsonConvert.SerializeObject(account), TimeSpan.FromMinutes(10));
            return account;
        }
        return null;
    }
}