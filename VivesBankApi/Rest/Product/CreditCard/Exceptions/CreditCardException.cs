namespace VivesBankApi.Rest.Product.CreditCard.Exceptions;

/// <summary>
/// Excepción base para los errores relacionados con tarjetas de crédito.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardException(string message) : System.Exception(message)
{
    
    /// <summary>
    /// Excepción que indica que no se ha encontrado la tarjeta de crédito con el ID especificado.
    /// </summary>
    public class CreditCardNotFoundException(string id)
        : CreditCardException($"The credit card with the ID {id} was not found");
    
    
    /// <summary>
    /// Excepción que indica que la tarjeta de crédito con el ID especificado no está asignada a ninguna cuenta.
    /// </summary>
    public class CreditCardNotAssignedException(string id)
        : CreditCardException($"The credit card with the ID {id} is not assigned to any account");
    
    /// <summary>
    /// Excepción que indica que no se ha encontrado la tarjeta de crédito con el número de tarjeta especificado.
    /// </summary>
    public class CreditCardNotFoundByCardNumberException(string cardNumber)
        : CreditCardException($"The credit card with card number {cardNumber} was not found");
}
