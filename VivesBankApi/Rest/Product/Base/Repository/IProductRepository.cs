using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Product.Base.Models;

/// <summary>
/// Defines the contract for interacting with product data in the data store.
/// This interface extends the generic repository functionality for the `Product` entity.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>
    /// Asynchronously retrieves a product by its name.
    /// </summary>
    /// <param name="name">The name of the product to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, with a result of the product if found, otherwise null.</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    Task<Product?> GetByNameAsync(string name);
}