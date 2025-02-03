using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;

namespace VivesBankApi.Rest.Product.Base.Repository;
/// <summary>
/// Repository class for interacting with `Product` entities in the database.
/// It extends the generic repository functionality to handle CRUD operations for the `Product` entity.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class ProductRepository : GenericRepository<BancoDbContext, Base.Models.Product>, IProductRepository
{
    /// <summary>
    /// Initializes a new instance of the `ProductRepository` class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance used for logging messages.</param>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    public ProductRepository(BancoDbContext context, ILogger<ProductRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Asynchronously retrieves a product from the database by its name.
    /// </summary>
    /// <param name="name">The name of the product to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, with a result of the product if found, otherwise null.</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    public async Task<Models.Product?> GetByNameAsync(string name)
    {
        _logger.LogInformation($"Searching product by name: {name}");   
        // Returns the first product that matches the given name, or null if not found
        return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
    }
}