using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Generators;


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

    public CreditCardService(ICreditCardRepository creditCardRepository, ILogger<CreditCardService> logger, CvcGenerator cvcGenerator, ExpirationDateGenerator expirationDateGenerator, NumberGenerator numberGenerator, IAccountsRepository accountsRepository, IConnectionMultiplexer connectionMultiplexer)
    {
        _logger = logger;
        _creditCardRepository = creditCardRepository;
        _cvcGenerator = cvcGenerator;
        _expirationDateGenerator = expirationDateGenerator;
        _numberGenerator = numberGenerator;
        _accountsRepository = accountsRepository;
        _cache = connectionMultiplexer.GetDatabase();
    }

    public async Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync()
    {
        _logger.LogInformation("Getting all credit cards");
        
        var creditCards = await _creditCardRepository.GetAllAsync();
        
        return creditCards.Select(creditCard => creditCard.ToAdminResponse()).ToList();
    }

    public async Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id)
    {
        _logger.LogInformation($"Getting card with id {id}");
        var creditCard = await GetByIdAsync(id) ?? throw new CreditCardException.CreditCardNotFoundException(id);
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
            _logger.LogError($"Card not found with id {id}");
            throw new CreditCardException.CreditCardNotFoundException($"Credit card with id '{id}' not found.");
        }
        return null;
    }

    public async Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest)
    {
        _logger.LogInformation($"Creating card: {createRequest}");
            
        var creditCardModel = CreditCardMapper.FromDtoRequest(createRequest);

        creditCardModel.CardNumber = _numberGenerator.GenerateCreditCardNumber();
        creditCardModel.ExpirationDate = _expirationDateGenerator.GenerateRandomDate();
        creditCardModel.Cvc = _cvcGenerator.Generate();
        
        var account = await _accountsRepository.getAccountByIbanAsync(createRequest.AccountIban);
        
        if (account == null)
        {
            _logger.LogError($"Account not found with iban {createRequest.AccountIban}");
            throw new Exception(createRequest.AccountIban);
        }
        
        creditCardModel.AccountId = account.Id;

        await _creditCardRepository.AddAsync(creditCardModel);
            
        return creditCardModel.ToClientResponse();
    }

    public async Task<CreditCardClientResponse> UpdateCreditCardAsync(string cardId, CreditCardUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating card: {updateRequest} by Id: {cardId}");
        
        var creditCard = await _creditCardRepository.GetByIdAsync(cardId);

        if (creditCard == null)
        {
            _logger.LogError($"Card not found with id {cardId}");
            throw new CreditCardException.CreditCardNotFoundException(cardId);
        }

        creditCard.Pin = updateRequest.Pin;
        creditCard.UpdatedAt = DateTime.Now;
        
        await _creditCardRepository.UpdateAsync(creditCard);
        await _cache.KeyDeleteAsync(cardId);
        await _cache.StringSetAsync(cardId, JsonConvert.SerializeObject(updateRequest), TimeSpan.FromMinutes(10));
        
        return creditCard.ToClientResponse();
    }

    public Task DeleteCreditCardAsync(string cardId)
    {
        _logger.LogInformation($"Removing card by Id: {cardId} ");
        _cache.KeyDeleteAsync(cardId);
        return _creditCardRepository.DeleteAsync(cardId);
    }
}