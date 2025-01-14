using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Database;
using VivesBankApi.Products.Models;

namespace VivesBankApi.Products.Repository;

public class ProductRepository : GenericRepository<BancoDbContext,Product>, IProductRepository
{
    public ProductRepository(BancoDbContext context, ILogger<ProductRepository> logger)
        : base(context, logger)
    {
        
    }
}