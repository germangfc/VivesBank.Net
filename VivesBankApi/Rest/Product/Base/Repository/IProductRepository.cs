using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.Base.Models;

public interface IProductRepository : IGenericRepository<Product>
{
    public Task<Product?> GetByNameAsync(string name);
}