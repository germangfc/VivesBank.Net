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

    public Task<PagedList<CreditCard>> GetAllClientsPagedAsync(int pageNumber, int pageSize, string name, bool? isDeleted, string direction)
    {
        throw new NotImplementedException();
    }
}