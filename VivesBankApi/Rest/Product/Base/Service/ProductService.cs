using System.Reactive.Linq;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Mapper;
using VivesBankApi.Rest.Product.Base.Validators;
using VivesBankApi.Rest.Product.Service;
using VivesBankApi.Utils.GenericStorage.JSON;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.Rest.Product.Base.Service;

public class ProductService : GenericStorageJson<Models.Product>, IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ProductValidator _productValidator;
    private readonly IWebsocketHandler _websocketHandler;
    private readonly IDatabase _cache;

    // Constructor de ProductService
    public ProductService(
        ILogger<ProductService> logger, 
        IProductRepository productRepository, 
        ProductValidator productValidator, 
        IConnectionMultiplexer connection, 
        IWebsocketHandler websocketHandler)
        : base(logger)
    {
        _productRepository = productRepository;
        _productValidator = productValidator;
        _cache = connection.GetDatabase();
        _websocketHandler = websocketHandler;
    }
    
    public async Task<List<Models.Product>> GetAll()
    {
        return await _productRepository.GetAllAsync();
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
        await EnviarNotificacionGlobalCreateAsync(productModel.ToDtoResponse());
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
    
    public async Task EnviarNotificacionGlobalCreateAsync<T>(T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Create.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };
        await _websocketHandler.NotifyAllAsync(notificacion);
    }

    public List<Base.Models.Product> LoadCsv(Stream stream)
    {
        Console.WriteLine("Loading products from CSV file...");

        try
        {
            var products = new List<Base.Models.Product>();

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                bool isFirstLine = true;

                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (isFirstLine)
                    {
                        isFirstLine = false;
                        continue;
                    }

                    var data = line.Split(',');

                    if (data.Length < 6)
                    {
                        Console.WriteLine($"Skipping invalid line: {line}");
                        continue;
                    }

                    var product = new Base.Models.Product(
                        name: data[1].Trim(),
                        productType: Enum.Parse<Base.Models.Product.Type>(data[2].Trim(), true)
                    )
                    {
                        Id = data[0].Trim(),
                        CreatedAt = DateTime.Parse(data[3].Trim()),
                        UpdatedAt = DateTime.Parse(data[4].Trim()),
                        IsDeleted = bool.Parse(data[5].Trim())
                    };

                    products.Add(product);
                }
            }

            return products;
        }
        catch (System.Exception ex)
        {
            Console.WriteLine($"Error processing CSV file: {ex.Message}");
            return new List<Base.Models.Product>(); 
        }
    }

    public IObservable<Base.Models.Product> Import(IFormFile fileStream)
    {
        _logger.LogInformation("Importing Products from JSON file...");
    
        return Observable.Create<Base.Models.Product>(async (observer, cancellationToken) =>
        {
            try
            {
                using var stream = fileStream.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader)
                {
                    SupportMultipleContent = true
                };

                var serializer = new JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var product = serializer.Deserialize<Base.Models.Product>(jsonReader);
                        observer.OnNext(product);
                    }
                }

                observer.OnCompleted();
            }
            catch (System.Exception ex)
            {
                observer.OnError(ex);
            }
        });
    }
    
    public async Task<FileStream> Export(List<Base.Models.Product> entities)
    {
        _logger.LogInformation("Exporting Products to JSON file...");

        var json = JsonConvert.SerializeObject(entities, Formatting.Indented);

        var directoryPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = "ProductsInSystem-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        var filePath = System.IO.Path.Combine(directoryPath, fileName);

        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation($"File written to: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}