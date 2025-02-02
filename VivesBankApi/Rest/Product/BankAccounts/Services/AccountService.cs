using System.Reactive.Linq;
using System.Security.Claims;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Movimientos.Validators;
using VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.Utils.GenericStorage.JSON;
using VivesBankApi.Utils.IbanGenerator;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.Rest.Product.BankAccounts.Services;

public class AccountService : GenericStorageJson<Account>, IAccountsService
{
    private readonly IAccountsRepository _accountsRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;
    private readonly IIbanGenerator _ibanGenerator;
    private readonly IDatabase _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IWebsocketHandler _websocketHandler;

    public AccountService(
        ILogger<AccountService> logger, 
        IIbanGenerator ibanGenerator,
        IClientRepository clientRepository,
        IProductRepository productRepository,
        IAccountsRepository accountsRepository, 
        IConnectionMultiplexer connection, 
        IHttpContextAccessor httpContextAccessor, 
        IUserService userService, 
        IWebsocketHandler websocketHandler)
        : base(logger)
    {
        _ibanGenerator = ibanGenerator;
        _accountsRepository = accountsRepository;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
        _cache = connection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _websocketHandler = websocketHandler;
    }
    
    public async Task<List<Account>> GetAll()
    {
        return await _accountsRepository.GetAllAsync();
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

    public async Task<List<AccountResponse>> GetMyAccountsAsClientAsync()
    {
        _logger.LogInformation("Getting my accounts as client");
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id) ?? throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(id) ?? throw new ClientExceptions.ClientNotFoundException(id);
        var accounts = await _accountsRepository.getAccountByClientIdAsync(client.Id);
        return accounts.Select(a => a.toResponse()).ToList();
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
        _logger.LogInformation("Creating account for Client registered on the system");

        if (_httpContextAccessor.HttpContext == null)
            throw new Exception("HttpContext is null");

        var user = _httpContextAccessor.HttpContext.User;

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id))
            throw new Exception("User ID claim is missing");

        var userForFound = await _userService.GetUserByIdAsync(id)
                           ?? throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(id)
                     ?? throw new ClientExceptions.ClientNotFoundException(id);

        var product = await _productRepository.GetByNameAsync(request.ProductName);
        if (product == null)
            throw new AccountsExceptions.AccountNotCreatedException();

        var iban = await _ibanGenerator.GenerateUniqueIbanAsync();
        if (string.IsNullOrWhiteSpace(iban))
            throw new Exception("IBAN generation failed");

        var account = request.fromDtoRequest();
        account.ClientId = client.Id;
        account.ProductId = product.Id;
        account.IBAN = iban;
        account.Balance = 0;

        await _accountsRepository.AddAsync(account);
        await EnviarNotificacionCreateAsync(userForFound, account.toResponse());

        return account.toResponse();
    }

    
    public async Task DeleteMyAccountAsync(String iban)
    {
        _logger.LogInformation("Deleting my account with iban: " +iban);
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id) ?? throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(id) ?? throw new ClientExceptions.ClientNotFoundException(id);
        var accountToDelete = await _accountsRepository.getAccountByIbanAsync(iban) ?? throw new AccountsExceptions.AccountNotFoundException(iban);
        if(accountToDelete.ClientId!= client.Id)
            throw new AccountsExceptions.AccountNotDeletedException(iban);
        if (accountToDelete.Balance > 0) throw new AccountsExceptions(iban);
        accountToDelete.IsDeleted = true;
        await _accountsRepository.UpdateAsync(accountToDelete);
        await EnviarNotificacionDeleteAsync(userForFound, accountToDelete.toResponse());
        await _cache.KeyDeleteAsync(id);
        await _cache.KeyDeleteAsync("account:" + accountToDelete.IBAN);
    }

    public async Task<AccountCompleteResponse> UpdateAccountAsync(string id, UpdateAccountRequest request)
    {
        _logger.LogInformation($"Updating account with id {id}");
        
        var account = await GetByIdAsync(id) ?? throw new AccountsExceptions.AccountNotFoundException(id);
        
        if (await _productRepository.GetByIdAsync(request.ProductID) == null)
            throw new AccountsExceptions.AccountNotUpdatedException(id);
        
        if (await _clientRepository.GetByIdAsync(request.ClientID) == null)
            throw new AccountsExceptions.AccountNotCreatedException();

        if (!IbanValidator.ValidateIban(request.IBAN)) 
            throw new AccountsExceptions.AccountIbanNotValid(request.IBAN);

        if (!Enum.IsDefined(typeof(AccountType), request.AccountType)) 
            throw new AccountsExceptions.AccountNotUpdatedException(id);

        account.Balance = request.Balance;
        account.UpdatedAt = DateTime.UtcNow;
        
        await _accountsRepository.UpdateAsync(account);
        return account.toCompleteResponse();
        
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
    
    public async Task EnviarNotificacionCreateAsync<T>(UserResponse user, T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Create.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };
        await _websocketHandler.NotifyUserAsync(user.Id, notificacion);
    }public async Task EnviarNotificacionDeleteAsync<T>(UserResponse user, T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Delete.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };
        await _websocketHandler.NotifyUserAsync(user.Id, notificacion);
    }

    public IObservable<Account> Import(IFormFile fileStream)
    {
        _logger.LogInformation("Starting to import accounts from JSON file.");

        return Observable.Create<Account>(async (observer, cancellationToken) =>
        {
            try
            {
                using var stream = fileStream.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader) { SupportMultipleContent = true };

                var serializer = new JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var account = serializer.Deserialize<Account>(jsonReader);
                    
                        if (account != null)
                        {
                            _logger.LogInformation($"Deserialized Account: {account.Id}"); 
                            observer.OnNext(account);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize an account.");
                        }
                    }
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while processing the JSON file: {ex.Message}");
                observer.OnError(ex);
            }
        });
    }


    
    public async Task<FileStream> Export(List<Account> entities)
    {
        _logger.LogInformation("Exporting Accounts to JSON file...");

        var json = JsonConvert.SerializeObject(entities, Formatting.Indented);

        var directoryPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = "BankAccountInSystem-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        var filePath = System.IO.Path.Combine(directoryPath, fileName);

        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation($"File written to: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}