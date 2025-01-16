using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


public static class CreditCardMapper
{
    public static CreditCardAdminResponse ToAdminResponse(this CreditCard creditCard)
    {
        return new CreditCardAdminResponse
        {
            CardNumber = creditCard.CardNumber,
            ExpirationDate = creditCard.ExpirationDate.ToString(),
        };
    }

    public static CreditCard FromDtoRequest(this CreditCardRequest request)
    {
        return new CreditCard
        {
            Pin = request.Pin
        };
    }
    
    public static CreditCardClientResponse ToClientResponse(this CreditCard creditCard)
    {
        return new CreditCardClientResponse
        {
            CardNumber = creditCard.CardNumber,
            ExpirationDate = creditCard.ExpirationDate.ToString(),
        };
    }

}