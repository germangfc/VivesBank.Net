using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public class CreditCardService : ICreditCardService
{
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly ILogger _logger;

    public CreditCardService(ICreditCardRepository creditCardRepository, ILogger<CreditCardService> logger)
    {
        _logger = logger;
        _creditCardRepository = creditCardRepository;
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
        
        var creditCard = await _creditCardRepository.GetByIdAsync(id);

        if (creditCard == null)
        {
            _logger.LogError($"Card not found with id {id}");
            throw new CreditCardException.CreditCardNotFoundException(id);
        }
        
        return creditCard.ToAdminResponse();
    }

    public async Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest)
    {
        _logger.LogInformation($"Creating card: {createRequest}");
        
        var creditCardModel = CreditCardMapper.FromDtoRequest(createRequest);
        await _creditCardRepository.AddAsync(creditCardModel);
        
        return creditCardModel.ToClientResponse();
    }

    public Task<CreditCardClientResponse> UpdateCreditCardAsync(string cardId, CreditCardRequest updateRequest)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCreditCardAsync(string cardId)
    {
        throw new NotImplementedException();
    }
}