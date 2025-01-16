namespace VivesBankApi.Rest.Product.CreditCard.Exceptions;

public class CreditCardException(string message) : System.Exception(message)
{
    public class CreditCardNotFoundException(string id)
        : CreditCardException($"The credit card with the ID {id} was not found");

    public class CreditCardInvalidTypeException(string invalidType)
        : CreditCardException($"Invalid credit card type: {invalidType}");
}
