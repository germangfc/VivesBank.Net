using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Storage;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.Service;

/// <summary>
/// Define el contrato para interactuar con los datos de productos en el almacén de datos.
/// Esta interfaz extiende la funcionalidad de almacenamiento genérico para la entidad `Product`,
/// admitiendo operaciones con mecanismos de almacenamiento en CSV y JSON.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public interface IProductService : IStorageCsv, IGenericStorageJson<Base.Models.Product>
{
    /// <summary>
    /// Recupera todos los productos del almacén de datos.
    /// </summary>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene una lista de todos los productos.</returns>
    Task<List<Base.Models.Product>> GetAll();

    /// <summary>
    /// Recupera todos los productos como una lista de objetos <see cref="ProductResponse"/>.
    /// </summary>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene una lista de respuestas de productos.</returns>
    Task<List<ProductResponse>> GetAllProductsAsync();

    /// <summary>
    /// Recupera un producto específico por su identificador único.
    /// </summary>
    /// <param name="productId">El identificador único del producto.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene la respuesta del producto si se encuentra; de lo contrario, null.</returns>
    Task<ProductResponse?> GetProductByIdAsync(String productId);

    /// <summary>
    /// Crea un nuevo producto en el almacén de datos.
    /// </summary>
    /// <param name="createRequest">El objeto de solicitud que contiene los detalles del producto.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene la respuesta del producto creado.</returns>
    Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest);

    /// <summary>
    /// Actualiza un producto existente en el almacén de datos.
    /// </summary>
    /// <param name="productId">El identificador único del producto a actualizar.</param>
    /// <param name="updateRequest">El objeto de solicitud que contiene los detalles actualizados del producto.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea contiene la respuesta del producto actualizado si la actualización fue exitosa; de lo contrario, null.</returns>
    Task<ProductResponse?> UpdateProductAsync(String productId, ProductUpdateRequest updateRequest);

    /// <summary>
    /// Elimina un producto del almacén de datos.
    /// </summary>
    /// <param name="productId">El identificador único del producto a eliminar.</param>
    /// <returns>Una tarea que representa la operación asincrónica. El resultado de la tarea es true si el producto se eliminó con éxito; de lo contrario, false.</returns>
    Task<bool> DeleteProductAsync(String productId);
}