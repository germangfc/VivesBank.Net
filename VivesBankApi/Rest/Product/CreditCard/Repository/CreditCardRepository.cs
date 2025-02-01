using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.CreditCard.Models;


public class CreditCardRepository : GenericRepository<BancoDbContext, CreditCard>, ICreditCardRepository
{
    public CreditCardRepository(BancoDbContext context, ILogger<CreditCardRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<CreditCard?> GetByCardNumber(string cardNumber)
    {
        _logger.LogInformation($"Getting credit card with card number: {cardNumber}");
        return await _dbSet.FirstOrDefaultAsync(a => a.CardNumber == cardNumber);
    }

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