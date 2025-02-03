using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


namespace VivesBankApi.Rest.Product.CreditCard.Mappers;

using Models;
/// <summary>
/// Clase estática que contiene métodos de extensión para convertir entre diferentes representaciones de la entidad `CreditCard`.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public static class CreditCardMapper
{
    /// <summary>
    /// Convierte un objeto de tipo `CreditCard` a `CreditCardAdminResponse` para ser utilizado en respuestas de administración.
    /// </summary>
    /// <param name="creditCard">El objeto de tipo `CreditCard` que se va a convertir.</param>
    /// <returns>Un objeto de tipo `CreditCardAdminResponse` que representa los datos de la tarjeta para el administrador.</returns>
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

    /// <summary>
    /// Convierte un objeto de tipo `CreditCardRequest` a un objeto de tipo `CreditCard`.
    /// Este método se utiliza para mapear una solicitud de creación de tarjeta a un objeto `CreditCard`.
    /// </summary>
    /// <param name="request">El objeto de tipo `CreditCardRequest` que se va a convertir.</param>
    /// <returns>Un objeto de tipo `CreditCard` que representa la tarjeta con los datos del request.</returns>
    public static CreditCard FromDtoRequest(this CreditCardRequest request)
    {
        return new CreditCard
        {
            Pin = request.Pin,
            CardNumber = null, // Se deja como null ya que no es proporcionado en la solicitud
            ExpirationDate = default, // Se deja por defecto ya que no es proporcionada en la solicitud
            Cvc = null, // Se deja como null ya que no es proporcionado en la solicitud
            AccountId = null // Se deja como null ya que no es proporcionado en la solicitud
        };
    }

    /// <summary>
    /// Convierte un objeto de tipo `CreditCard` a `CreditCardClientResponse` para ser utilizado en respuestas de cliente.
    /// </summary>
    /// <param name="creditCard">El objeto de tipo `CreditCard` que se va a convertir.</param>
    /// <returns>Un objeto de tipo `CreditCardClientResponse` que representa los datos de la tarjeta para el cliente.</returns>
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

    /// <summary>
    /// Convierte un objeto de tipo `CreditCardClientResponse` a `CreditCard`, usado para actualizar los datos de la tarjeta.
    /// </summary>
    /// <param name="clientResponse">El objeto de tipo `CreditCardClientResponse` que se va a convertir.</param>
    /// <returns>Un objeto de tipo `CreditCard` con los datos del `CreditCardClientResponse`.</returns>
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
