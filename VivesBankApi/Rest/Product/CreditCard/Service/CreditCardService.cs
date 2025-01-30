using System.Security.Claims;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Service;


namespace VivesBankApi.Rest.Product.CreditCard.Service;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ILogger _logger;
    private readonly CvcGenerator _cvcGenerator;
    private readonly ExpirationDateGenerator _expirationDateGenerator;
    private readonly NumberGenerator _numberGenerator;
    private readonly IAccountsRepository _accountsRepository;
    private readonly IDatabase _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserService _userService;
    private readonly IClientRepository _clientRepository;

    public CreditCardService(ICreditCardRepository creditCardRepository, ILogger<CreditCardService> logger, CvcGenerator cvcGenerator, ExpirationDateGenerator expirationDateGenerator, NumberGenerator numberGenerator, IAccountsRepository accountsRepository, IConnectionMultiplexer connectionMultiplexer, IHttpContextAccessor httpContextAccessor, IUserService userService, IClientRepository clientRepository)
    {
        _logger = logger;
        _creditCardRepository = creditCardRepository;
        _cvcGenerator = cvcGenerator;
        _expirationDateGenerator = expirationDateGenerator;
        _numberGenerator = numberGenerator;
        _accountsRepository = accountsRepository;
        _cache = connectionMultiplexer.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _userService = userService;
        _clientRepository = clientRepository;
    }

    public async Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync(int pageNumber, 
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction)
    {
        _logger.LogInformation("Getting all credit cards");
        var cards = await _creditCardRepository.GetAllCrediCardsPaginated(pageNumber, pageSize, fullName, isDeleted, direction);
        var mappedCards = new PagedList<CreditCardAdminResponse>(
            cards.Select(u => u.ToAdminResponse()),
            cards.TotalCount,
            cards.PageNumber,
            cards.PageSize
        );
        return mappedCards;
    }

    public async Task<List<CreditCardClientResponse>> GetMyCreditCardsAsync()
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
        if (userForFound == null)
            throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(userForFound.Id);
        if (client == null)
            throw new ClientExceptions.ClientNotFoundException(userForFound.Id);
        var accounts = await _accountsRepository.getAccountByClientIdAsync(client.Id);
        if (accounts == null || !accounts.Any())
            return new List<CreditCardClientResponse>();
        var creditCards = new List<CreditCardClientResponse>();
        foreach (var account in accounts)
        {
            var creditCard = await _creditCardRepository.GetCardsByAccountId(account.Id);
            if (creditCard != null)
            {
                var creditCardResponse = creditCard.ToClientResponse();
                creditCards.Add(creditCardResponse);
            }
        }
        return creditCards;
    }

    public async Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id)
    {
        _logger.LogInformation($"Getting card with id {id}");
        var creditCard = await GetByIdAsync(id) ?? throw new CreditCardException.CreditCardNotFoundException(id);
        return creditCard.ToAdminResponse();
    }

    public async Task<CreditCardAdminResponse?> GetCreditCardByCardNumber(string cardNumber)
    {
        _logger.LogInformation($"Getting card with card number {cardNumber}");
        var creditCard = await _creditCardRepository.GetByCardNumber(cardNumber) 
                         ?? throw new CreditCardException.CreditCardNotFoundException(cardNumber);
        return creditCard.ToAdminResponse();
    }

    private async Task<Models.CreditCard?> GetByIdAsync(string id)
    {
        var cacheCreditCard = await _cache.StringGetAsync(id);
        if (!cacheCreditCard.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Models.CreditCard>(cacheCreditCard);
        }

        Models.CreditCard? creditCard = await _creditCardRepository.GetByIdAsync(id);

        if (creditCard != null)
        {
            await _cache.StringSetAsync(id, JsonConvert.SerializeObject(creditCard), TimeSpan.FromMinutes(10));
            return creditCard;
        }
        return null;
    }

    public async Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
        if (userForFound == null)
            throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(userForFound.Id);
        if (client == null)
            throw new ClientExceptions.ClientNotFoundException(userForFound.Id);
        _logger.LogInformation($"Creating card: {createRequest}");
            
        var creditCardModel = CreditCardMapper.FromDtoRequest(createRequest);

        creditCardModel.CardNumber = _numberGenerator.GenerateCreditCardNumber();
        creditCardModel.ExpirationDate = _expirationDateGenerator.GenerateRandomDate();
        creditCardModel.Cvc = _cvcGenerator.Generate();
        
        var account = await _accountsRepository.getAccountByIbanAsync(createRequest.AccountIban);
        
        if (account == null)
        {
            _logger.LogError($"Account not found with iban {createRequest.AccountIban}");
            throw new AccountsExceptions.AccountNotFoundByIban(createRequest.AccountIban);
        }

        if (account.ClientId == client.Id)
        {
            _logger.LogError($"Client does not have access to account with iban {createRequest.AccountIban}");
            throw new ClientExceptions.ClientNotAllowedToAccessAccount(userForFound.Id, createRequest.AccountIban);
        }
        
        creditCardModel.AccountId = account.Id;

        await _creditCardRepository.AddAsync(creditCardModel);
            
        return creditCardModel.ToClientResponse();
    }

    public async Task<CreditCardClientResponse> UpdateCreditCardAsync(string cardNumber, CreditCardUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating card: {updateRequest} by Id: {cardNumber}");
        
        var myCreditCards = await GetMyCreditCardsAsync();
        var creditCard = await _creditCardRepository.GetByCardNumber(cardNumber);

        if (creditCard == null)
        {
            _logger.LogError($"Card not found with id {cardNumber}");
            throw new CreditCardException.CreditCardNotFoundException(cardNumber);
        }
        
        var cardToUpdate = myCreditCards.FirstOrDefault(card => card.AccountId == creditCard.AccountId);
        if (cardToUpdate == null)
        {
            _logger.LogError($"Card not found with account id {creditCard.AccountId}");
            throw new CreditCardException.CreditCardNotFoundException(cardNumber);
        }
        
        creditCard.Pin = updateRequest.Pin;
        creditCard.UpdatedAt = DateTime.UtcNow;
    
        await _creditCardRepository.UpdateAsync(creditCard);
        
        await _cache.KeyDeleteAsync(cardNumber);
        await _cache.StringSetAsync(cardNumber, JsonConvert.SerializeObject(updateRequest), TimeSpan.FromMinutes(10));

        return creditCard.ToClientResponse();
    }


    public async Task DeleteCreditCardAsync(string number)
    {
        _logger.LogInformation($"Removing card by number: {number} ");
        var myCreditCards = await GetMyCreditCardsAsync();
        var myCardToDelete = myCreditCards.FirstOrDefault(card => card.CardNumber == number)??
                           throw new CreditCardException.CreditCardNotFoundByCardNumberException(number);
        _cache.KeyDeleteAsync(myCardToDelete.Id);
        var deletedCard = await _creditCardRepository.GetByCardNumber(number) ?? throw new CreditCardException.CreditCardNotFoundException(number);
        deletedCard.IsDeleted = true;
        await _creditCardRepository.UpdateAsync(deletedCard);
        
    }
    
}