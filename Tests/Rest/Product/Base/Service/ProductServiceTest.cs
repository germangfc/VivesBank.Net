using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Models;
using VivesBankApi.Rest.Product.Base.Repository;
using VivesBankApi.Rest.Product.Base.Validators;
using VivesBankApi.Rest.Product.Service;
using VivesBankApi.WebSocket.Service;

[TestFixture]
[TestOf(typeof(ProductService))]
public class ProductServiceTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private BancoDbContext _dbContext;
    private ProductRepository _repository;
    private ProductService _productService;
    private ProductValidator _productValidator;
    private Mock<IDatabase> _cache;
    private Mock<IConnectionMultiplexer> connection;
    private Mock<IWebsocketHandler> _websocketHandler;
    
    [OneTimeSetUp]
    public async Task Setup()
    {
        _websocketHandler = new Mock<IWebsocketHandler>();
        connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        connection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<string>())).Returns(_cache.Object);
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();

        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<BancoDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new BancoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _repository = new ProductRepository(_dbContext, NullLogger<ProductRepository>.Instance);
        _productService = new ProductService(NullLogger<ProductService>.Instance, _repository, _productValidator, connection.Object, _websocketHandler.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
        _cache.Reset();
    }
    
    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }
    
    [Test]
    public async Task GetAllProducts()
    {
        var product1 = new Product("Producto 1", Product.Type.BankAccount);
        var product2 = new Product("Producto 2", Product.Type.CreditCard);
        _dbContext.Products.AddRange(product1, product2);
        await _dbContext.SaveChangesAsync();

        var products = await _productService.GetAllProductsAsync();

        Assert.That(products, Is.Not.Null);
        Assert.That(products.Count, Is.GreaterThan(0));
        Assert.That(products.Any(p => p.Name == "Producto 1"), Is.True);
        Assert.That(products.Any(p => p.Name == "Producto 2"), Is.True);
    }
    
    [Test]
    public async Task GetProductById()
    {
        var product = new Product("Producto Test", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _productService.GetProductByIdAsync(product.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Producto Test"));
    }
    
    [Test]
    public async Task GetProductById_WhenFoundInCache()
    {
        var product = new Product("Producto Test", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();
        
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonSerializer.Serialize(product));
        var result = await _productService.GetProductByIdAsync(product.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Producto Test")); 
        _cache.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Exactly(1));
    }
    
    [Test]
    public void GetProductByIdProductNotFound()
    {
        var nonExistingProductId = "ProductoNoExiste";

        var ex = Assert.ThrowsAsync<ProductException.ProductNotFoundException>(
            async () => await _productService.GetProductByIdAsync(nonExistingProductId)
        );

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo($"The product with the ID {nonExistingProductId} was not found"));
        _cache.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
    }

    [Test]
    public async Task CreateProduct()
    {
        var createRequest = new ProductCreateRequest {Name = "Producto Nuevo", Type = "BANKACCOUNT" };
        ProductValidator.isValidProduct(createRequest);
        var result = await _productService.CreateProductAsync(createRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Producto Nuevo"));
        Assert.That(result.Type, Is.EqualTo("BankAccount"));
    }

    [Test]
    public async Task UpdateProduct()
    {
        var product = new Product("Producto Test", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new ProductUpdateRequest { Name = "Producto Actualizado" };
        var result = await _productService.UpdateProductAsync(product.Id, updateRequest);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("Producto Actualizado"));
        _cache.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
        _cache.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
    }

    [Test]
    public void UpdateProductProductNotFound()
    {
        var nonExistingProductId = "ProductoNoExiste";
        var updateRequest = new ProductUpdateRequest { Name = "Producto Actualizado" };

        var ex = Assert.ThrowsAsync<ProductException.ProductNotFoundException>(
            async () => await _productService.UpdateProductAsync(nonExistingProductId, updateRequest)
        );

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo($"The product with the ID {nonExistingProductId} was not found"));
        _cache.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
    }

    
    [Test]
    public async Task DeleteProduct()
    {
        var product = new Product("Producto a Eliminar", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        await _productService.DeleteProductAsync(product.Id);

        var deletedProduct = await _dbContext.Products.FindAsync(product.Id);
        Assert.That(deletedProduct, Is.Null);
        _cache.Verify(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
    }


    [Test]
    public void DeleteProductNotFound()
    {
        var nonExistentProductId = "ProductoNoExiste";

        var ex = Assert.ThrowsAsync<ProductException.ProductNotFoundException>(
            async () => await _productService.DeleteProductAsync(nonExistentProductId)
        );

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Is.EqualTo($"The product with the ID {nonExistentProductId} was not found"));
        _cache.Verify(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once());
    }

}