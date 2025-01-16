using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.CreditCard.Models;


public class CreditCardRepository : GenericRepository<BancoDbContext, CreditCard>, ICreditCardRepository
{
    public CreditCardRepository(BancoDbContext context, ILogger<CreditCardRepository> logger)
        : base(context, logger)
    {
    }
    
}