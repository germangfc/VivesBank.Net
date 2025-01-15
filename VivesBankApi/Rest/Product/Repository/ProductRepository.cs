using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.Models;


public class ProductRepository : GenericRepository<BancoDbContext,Product>, IProductRepository
{
    public ProductRepository(BancoDbContext context, ILogger<ProductRepository> logger)
        : base(context, logger)
    {
        
    }
}