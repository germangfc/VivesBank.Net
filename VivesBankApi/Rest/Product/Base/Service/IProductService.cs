using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Storage;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Product.Service;

public interface IProductService : IStorageCsv, IGenericStorageJson<Base.Models.Product>
{
    Task<List<ProductResponse>> GetAllProductsAsync();
    Task<ProductResponse?> GetProductByIdAsync(String productId);
    Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest);
    Task<ProductResponse?> UpdateProductAsync(String productId, ProductUpdateRequest updateRequest);
    Task<bool> DeleteProductAsync(String productId);
}