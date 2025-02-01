using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


namespace VivesBankApi.Rest.Product.CreditCard.Mappers;

using Models;
public static class CreditCardMapper
{
    public static CreditCardAdminResponse ToAdminResponse(this CreditCard creditCard)
    {
        return new CreditCardAdminResponse
        {
            Id = creditCard.Id,
            CardNumber = creditCard.CardNumber,
            AccountId = creditCard.AccountId,
            ExpirationDate = creditCard.ExpirationDate.ToString(),
            IsDeleted = creditCard.IsDeleted
        };
    }

    public static CreditCard FromDtoRequest(this CreditCardRequest request)
    {
        return new CreditCard
        {
            Pin = request.Pin,
            CardNumber = null,
            ExpirationDate = default, 
            Cvc = null,
            AccountId = null
        };
    }
    
    public static CreditCardClientResponse ToClientResponse(this CreditCard creditCard)
    {
        return new CreditCardClientResponse
        {
            Id = creditCard.Id,
            Pin = creditCard.Pin,
            Cvc = creditCard.Cvc,
            AccountId = creditCard.AccountId,
            CardNumber = creditCard.CardNumber,
            ExpirationDate = creditCard.ExpirationDate.ToString(),
            IsDeleted = creditCard.IsDeleted
        };
    }

    public static CreditCard toCreditCard(this CreditCardClientResponse clientResponse)
    {
        return new CreditCard
        {
            Id = clientResponse.Id,
            Pin = clientResponse.Pin,
            Cvc = clientResponse.Cvc,
            AccountId = clientResponse.AccountId,
            CardNumber = clientResponse.CardNumber,
            ExpirationDate = DateOnly.Parse(clientResponse.ExpirationDate)
        };
    }

}