using VivesBankApi.Rest.Product.Dto;

namespace VivesBankApi.Rest.Product.Service;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllProductsAsync();
    Task<ProductResponse> GetProductByIdAsync(String productId);
    Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest);
    Task<ProductResponse> UpdateProductAsync(String productId, ProductUpdateRequest updateRequest);
    Task DeleteProductAsync(String productId);
}