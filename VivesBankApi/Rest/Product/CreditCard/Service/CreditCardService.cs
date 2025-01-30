using System.Reactive.Linq;
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

    public IObservable<Models.CreditCard> Import(IFormFile fileStream)
    {
        _logger.LogInformation("Starting to import credit cards from JSON file.");

        return Observable.Create<Models.CreditCard>(async (observer, cancellationToken) =>
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
                        // Deserializar el objeto CreditCard
                        var creditCard = serializer.Deserialize<Models.CreditCard>(jsonReader);
                        observer.OnNext(creditCard);
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


    public async Task<FileStream> Export(List<Models.CreditCard> entities)
    {
        _logger.LogInformation("Exporting Credit Cards to JSON file...");

        var json = JsonConvert.SerializeObject(entities, Formatting.Indented);

        var directoryPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = "CreditCardInSystem-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        var filePath = System.IO.Path.Combine(directoryPath, fileName);
        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation($"File written to: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}