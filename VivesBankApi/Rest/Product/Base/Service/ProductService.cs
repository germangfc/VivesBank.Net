using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Validators;

namespace VivesBankApi.Rest.Product.Service;

public class ProductService : IProductService
{
    private readonly ILogger<ProductService> _logger;
    private readonly IProductRepository _productRepository;
    private readonly ProductValidator _productValidator;
    private readonly IDatabase _cache;
    
    public ProductService(ILogger<ProductService> logger, IProductRepository productRepository, ProductValidator productValidator, IConnectionMultiplexer connection)
    {
        _logger = logger;
        _productRepository = productRepository;
        _productValidator = productValidator;
        _cache = connection.GetDatabase();
    }
    
    public async Task<List<ProductResponse>> GetAllProductsAsync()
    {
        _logger.LogInformation("Getting all products");
        
        var products = await _productRepository.GetAllAsync();
        
        return products.Select(product => product.ToDtoResponse()).ToList();        
    }

    public async Task<ProductResponse> GetProductByIdAsync(string productId)
    {
        _logger.LogInformation($"Getting product with id {productId}");
    
        var product = await GetByIdAsync(productId);

        if (product == null)
        {
            _logger.LogError($"Product not found with id {productId}");
            throw new ProductException.ProductNotFoundException(productId);
        }
    
        return product.ToDtoResponse();
    }


    public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest)
    {
        _logger.LogInformation($"Creating product: {createRequest}");

        if (!ProductValidator.isValidProduct(createRequest))
        {
            _logger.LogError("Invalid product data provided.");
            throw new ProductException.ProductInvalidTypeException("Invalid product paremeters");
        }

        var productModel = ProductMapper.FromDtoRequest(createRequest);
        await _productRepository.AddAsync(productModel);
    
        return productModel.ToDtoResponse();
    }

    public async Task<ProductResponse> UpdateProductAsync(string productId, ProductUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating product: {updateRequest} by Id: {productId}");
        
        var product = await GetByIdAsync(productId);
        
        if (product == null)
        {
            _logger.LogError($"Product not found with id {productId}");
            throw new ProductException.ProductNotFoundException(productId);
        }

        product.Name = updateRequest.Name;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _productRepository.UpdateAsync(product);
        await _cache.KeyDeleteAsync(productId);

        return product.ToDtoResponse();
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        _logger.LogInformation($"Removing product by Id: {productId}");

        var product = await GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogError($"Product not found with id {productId}");
            throw new ProductException.ProductNotFoundException(productId); 
        }

        await _productRepository.DeleteAsync(productId);
        await _cache.KeyDeleteAsync(productId);
        _logger.LogInformation($"Product with Id: {productId} removed successfully.");
        return true; 
    }
    
    private async Task<Base.Models.Product?> GetByIdAsync(string id)
    {
        // Try to get from cache first
        var cachedProduct = await _cache.StringGetAsync(id);
        if (!cachedProduct.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Base.Models.Product>(cachedProduct);
        }

        // If not in cache, get from DB and cache it
        Base.Models.Product? product = await _productRepository.GetByIdAsync(id);
        if (product != null)
        {
            await _cache.StringSetAsync(id, JsonConvert.SerializeObject(product), TimeSpan.FromMinutes(10));
            return product;
        }
        return null;
    }
}