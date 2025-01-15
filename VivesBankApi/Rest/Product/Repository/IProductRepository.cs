using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.Models;

public interface IProductRepository : IGenericRepository<Product>
{
    public Task<Product?> GetByNameAsync(string name);
}