namespace VivesBankApi.Rest.Product.CreditCard.Exceptions;

public class CreditCardException(string message) : System.Exception(message)
{
    public class CreditCardNotFoundException(string id)
        : CreditCardException($"The credit card with the ID {id} was not found");
    public class CreditCardNotFoundByCardNumberException(string cardNumber)
        : CreditCardException($"The credit card with card number {cardNumber} was not found");
}
