using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;

namespace VivesBankApi.Rest.Product.Repository;
public class ProductRepository : GenericRepository<BancoDbContext,Models.Product>, IProductRepository
{
    public ProductRepository(BancoDbContext context, ILogger<ProductRepository> logger)
        : base(context, logger)
    {
    }
    public async Task<Models.Product?> GetByNameAsync(string name)
    {
         _logger.LogInformation($"Searching product by name: {name}");   
         return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
    }
    
        
    
}