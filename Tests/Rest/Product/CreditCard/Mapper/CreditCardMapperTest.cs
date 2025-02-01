using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Mappers;

namespace Tests.Rest.Product.CreditCard.Mapper;

[TestFixture]
public class CreditCardMapperTest
{
    [Test]
    public void ToAdminResponseValidCreditCardReturnsAdminResponse()
    {
        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "Card123",
            CardNumber = "1234567890123456",
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2))
        };

        var result = creditCard.ToAdminResponse();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(creditCard.Id));
        Assert.That(result.CardNumber, Is.EqualTo(creditCard.CardNumber));
        Assert.That(result.ExpirationDate, Is.EqualTo(creditCard.ExpirationDate.ToString()));
    }

    [Test]
    public void FromDtoRequestValidRequestReturnsCreditCardModel()
    {
        var creditCardRequest = new CreditCardRequest
        {
            Pin = "5678"
        };

        var result = creditCardRequest.FromDtoRequest();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Pin, Is.EqualTo(creditCardRequest.Pin));
        Assert.That(result.CardNumber, Is.Null);
        Assert.That(result.Cvc, Is.Null);
        Assert.That(result.AccountId, Is.Null);
        Assert.That(result.ExpirationDate, Is.EqualTo(default(DateOnly)));
    }

    [Test]
    public void ToClientResponseValidCreditCardReturnsClientResponse()
    {
        var creditCard = new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
        {
            Id = "Card123",
            Pin = "5678",
            Cvc = "456",
            AccountId = "Account123",
            CardNumber = "1234567890123456",
            ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(2))
        };

        var result = creditCard.ToClientResponse();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(creditCard.Id));
        Assert.That(result.Pin, Is.EqualTo(creditCard.Pin));
        Assert.That(result.Cvc, Is.EqualTo(creditCard.Cvc));
        Assert.That(result.AccountId, Is.EqualTo(creditCard.AccountId));
        Assert.That(result.CardNumber, Is.EqualTo(creditCard.CardNumber));
        Assert.That(result.ExpirationDate, Is.EqualTo(creditCard.ExpirationDate.ToString()));
    }
}