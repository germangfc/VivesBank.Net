using VivesBankApi.Rest.Product.Base.Dto;

namespace VivesBankApi.Rest.Product.Service;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllProductsAsync();
    Task<ProductResponse?> GetProductByIdAsync(String productId);
    Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest);
    Task<ProductResponse?> UpdateProductAsync(String productId, ProductUpdateRequest updateRequest);
    Task<bool> DeleteProductAsync(String productId);
}