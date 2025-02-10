using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.CreditCard.Service;

public interface ICreditCardService : IGenericStorageJson<Models.CreditCard>
{
    /// <summary>
    /// Obtiene todas las tarjetas de crédito para administradores con soporte para paginación, filtrado por nombre completo,
    /// estado de eliminación y dirección de ordenamiento.
    /// </summary>
    /// <param name="pageNumber">El número de página para la paginación.</param>
    /// <param name="pageSize">El tamaño de página para la paginación.</param>
    /// <param name="fullName">El nombre completo para filtrar (opcional).</param>
    /// <param name="isDeleted">El estado de eliminación (opcional).</param>
    /// <param name="direction">La dirección de ordenamiento, puede ser "asc" o "desc".</param>
    /// <returns>Una tarea que representa la operación asincrónica, con la lista de respuestas para administradores.</returns>
    Task<List<CreditCardAdminResponse>> GetAllCreditCardAdminAsync(int pageNumber,
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction);

    /// <summary>
    /// Obtiene todas las tarjetas de crédito disponibles.
    /// </summary>
    /// <returns>Una tarea que representa la operación asincrónica, con la lista de tarjetas de crédito.</returns>
    Task<List<Models.CreditCard>> GetAll();

    /// <summary>
    /// Obtiene todas las tarjetas de crédito asociadas al cliente autenticado.
    /// </summary>
    /// <returns>Una tarea que representa la operación asincrónica, con la lista de respuestas para clientes.</returns>
    Task<List<CreditCardClientResponse?>> GetMyCreditCardsAsync();

    /// <summary>
    /// Obtiene una tarjeta de crédito por su ID para administradores.
    /// </summary>
    /// <param name="id">El identificador de la tarjeta de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con la tarjeta de crédito correspondiente.</returns>
    Task<CreditCardAdminResponse?> GetCreditCardByIdAdminAsync(string id);

    /// <summary>
    /// Obtiene una tarjeta de crédito mediante su número de tarjeta.
    /// </summary>
    /// <param name="cardNumber">El número de la tarjeta de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con la tarjeta de crédito correspondiente.</returns>
    Task<CreditCardAdminResponse?> GetCreditCardByCardNumber(string cardNumber);

    /// <summary>
    /// Crea una nueva tarjeta de crédito asociada a una cuenta.
    /// </summary>
    /// <param name="createRequest">Los datos necesarios para crear la tarjeta de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con la tarjeta de crédito creada.</returns>
    Task<CreditCardClientResponse> CreateCreditCardAsync(CreditCardRequest createRequest);

    /// <summary>
    /// Actualiza una tarjeta de crédito existente.
    /// </summary>
    /// <param name="cardNumber">El número de la tarjeta de crédito que se desea actualizar.</param>
    /// <param name="updateRequest">Los datos necesarios para actualizar la tarjeta de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con la tarjeta de crédito actualizada.</returns>
    Task<CreditCardClientResponse> UpdateCreditCardAsync(String cardNumber, CreditCardUpdateRequest updateRequest);

    /// <summary>
    /// Elimina una tarjeta de crédito por su ID.
    /// </summary>
    /// <param name="cardId">El identificador de la tarjeta de crédito a eliminar.</param>
    /// <returns>Una tarea que representa la operación asincrónica de eliminación.</returns>
    Task DeleteCreditCardAsync(String cardId);
}
