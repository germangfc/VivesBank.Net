using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.CreditCard.Models;


/// <summary>
/// Representa un repositorio para realizar operaciones CRUD sobre las tarjetas de crédito en la base de datos.
/// Esta clase hereda de `GenericRepository` y está especializada para la entidad `CreditCard`.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CreditCardRepository : GenericRepository<BancoDbContext, CreditCard>, ICreditCardRepository
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase `CreditCardRepository`.
    /// </summary>
    /// <param name="context">El contexto de la base de datos que se utilizará para las operaciones CRUD.</param>
    /// <param name="logger">El registrador utilizado para el registro de eventos y errores.</param>
    public CreditCardRepository(BancoDbContext context, ILogger<CreditCardRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Obtiene una tarjeta de crédito mediante su número de tarjeta.
    /// </summary>
    /// <param name="cardNumber">El número de la tarjeta de crédito a buscar.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de la tarjeta de crédito si se encuentra, de lo contrario `null`.</returns>
    public async Task<CreditCard?> GetByCardNumber(string cardNumber)
    {
        _logger.LogInformation($"Getting credit card with card number: {cardNumber}");
        return await _dbSet.FirstOrDefaultAsync(a => a.CardNumber == cardNumber);
    }

    /// <summary>
    /// Obtiene las tarjetas de crédito asociadas a un identificador de cuenta.
    /// </summary>
    /// <param name="accountId">El identificador de la cuenta a la que están asociadas las tarjetas de crédito.</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de las tarjetas de crédito asociadas a la cuenta.</returns>
    /// <exception cref="ArgumentException">Se lanza si el `accountId` es nulo o vacío.</exception>
    public async Task<CreditCard?> GetCardsByAccountId(string accountId)
    {
        _logger.LogInformation($"Fetching credit cards for account ID: {accountId}");
        
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty.", nameof(accountId));
        }
        
        var creditCards = await _dbSet
            .Where(c => c.AccountId == accountId && !c.IsDeleted).FirstOrDefaultAsync();

        return creditCards;
    }

    /// <summary>
    /// Obtiene una lista de tarjetas de crédito paginada, filtrada por número de tarjeta y estado de eliminación.
    /// </summary>
    /// <param name="pageNumber">El número de página para la paginación.</param>
    /// <param name="pageSize">El tamaño de cada página para la paginación.</param>
    /// <param name="cardNumber">El número de tarjeta para filtrar los resultados (opcional).</param>
    /// <param name="isDeleted">El estado de eliminación de las tarjetas (opcional).</param>
    /// <param name="direction">La dirección de ordenamiento, puede ser "asc" o "desc".</param>
    /// <returns>Una tarea que representa la operación asincrónica, con el resultado de las tarjetas de crédito paginadas.</returns>
    public async Task<PagedList<CreditCard>> GetAllCrediCardsPaginated(int pageNumber, int pageSize, string cardNumber, bool? isDeleted, string direction)
    {
        _logger.LogInformation("Fetching all credit cards");

        var query = _dbSet.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(cardNumber))
        {
            query = query.Where(c => c.CardNumber.Contains(cardNumber.Trim()));
        }

        if (isDeleted.HasValue)
        {
            query = query.Where(c => c.IsDeleted == isDeleted.Value);
        }
        
        query = direction.ToLower() switch
        {
            "desc" => query.OrderByDescending(c => c.CardNumber),
            _ => query.OrderBy(c => c.CardNumber)
        };

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
        var creditCards = await EntityFrameworkQueryableExtensions.ToListAsync(query);
        
        return new PagedList<CreditCard>(
            creditCards,
            await EntityFrameworkQueryableExtensions.CountAsync(_dbSet),
            pageNumber,
            pageSize
        );
    }
}
