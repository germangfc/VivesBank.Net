using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Models;


/// <summary>
/// Define los métodos para interactuar con las tarjetas de crédito en el repositorio.
/// Hereda de `IGenericRepository<CreditCard>`, añadiendo operaciones específicas para las tarjetas de crédito.
/// </summary>
public interface ICreditCardRepository : IGenericRepository<CreditCard>
{
    /// <summary>
    /// Obtiene una tarjeta de crédito mediante su número de tarjeta.
    /// </summary>
    /// <param name="cardNumber">El número de la tarjeta de crédito a buscar.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de la tarjeta de crédito si se encuentra, de lo contrario `null`.</returns>
    Task<CreditCard?> GetByCardNumber(string cardNumber);

    /// <summary>
    /// Obtiene las tarjetas de crédito asociadas a un identificador de cliente.
    /// </summary>
    /// <param name="clientId">El identificador del cliente asociado a las tarjetas de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de las tarjetas de crédito asociadas al cliente.</returns>
    Task<CreditCard?> GetCardsByAccountId(string clientId);

    /// <summary>
    /// Obtiene una lista de tarjetas de crédito paginada, filtrada por nombre, estado de eliminación y dirección de ordenamiento.
    /// </summary>
    /// <param name="pageNumber">El número de página para la paginación.</param>
    /// <param name="pageSize">El tamaño de cada página para la paginación.</param>
    /// <param name="name">El nombre para filtrar los resultados (opcional).</param>
    /// <param name="isDeleted">El estado de eliminación de las tarjetas (opcional).</param>
    /// <param name="direction">La dirección de ordenamiento, puede ser "asc" o "desc".</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de las tarjetas de crédito paginadas.</returns>
    Task<PagedList<CreditCard>> GetAllCrediCardsPaginated(
        int pageNumber,
        int pageSize,
        string name,
        bool? isDeleted,
        string direction);
}
