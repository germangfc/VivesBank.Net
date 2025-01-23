using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
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
    
    public AccountService(ILogger<AccountService> logger, IIbanGenerator ibanGenerator,IClientRepository clientRepository,IProductRepository productRepository ,IAccountsRepository accountsRepository)
    {
        _logger = logger;
        _ibanGenerator = ibanGenerator;
        _accountsRepository = accountsRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
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
        // mal ??? var result = await _accountsRepository.GetByIdAsync(iban);
        var result = await _accountsRepository.getAccountByIbanAsync(iban);
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

    public async Task DeleteAccountAsync(string id)
    {
        _logger.LogInformation($"Deleting account with ID {id}");
        var result = await _accountsRepository.GetByIdAsync(id);
        if (result == null) throw new AccountsExceptions.AccountNotFoundException(id);
        result.IsDeleted = true;
        await _accountsRepository.UpdateAsync(result);
    }
}