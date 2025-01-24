using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Product.Base.Controller;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Models;
using VivesBankApi.Rest.Product.Base.Repository;
using VivesBankApi.Rest.Product.Base.Validators;
using VivesBankApi.Rest.Product.Service;


[TestFixture]
[TestOf(typeof(ProductController))]
public class ProductControllerTests
{
    private Mock<IDatabase> _cache;
    private Mock<IConnectionMultiplexer> connection;
    private PostgreSqlContainer _postgreSqlContainer;
    private BancoDbContext _dbContext;
    private ProductRepository _repository;
    private ProductService _service;
    private ProductController _controller;
    private ProductValidator _productValidator;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _cache = new Mock<IDatabase>();
        connection = new Mock<IConnectionMultiplexer>();
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
        _service = new ProductService(NullLogger<ProductService>.Instance, _repository, _productValidator, connection.Object);
        _controller = new ProductController(_service, NullLogger<ProductController>.Instance);
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }

    [Test]
    public async Task GetAllProducts()
    {
        var product = new Product("Product 1", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetAllProductsAsync();

        Assert.That(result, Is.InstanceOf<ActionResult<List<ProductResponse>>>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf<List<ProductResponse>>());
        Assert.That(okResult.Value, Is.Not.Null);
    }
    
    [Test]
    public async Task GetProductById()
    {
        var product = new Product("Product 3", Product.Type.BankAccount);
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _controller.GetProductByIdAsync(product.Id);

        Assert.That(result, Is.InstanceOf<ActionResult<ProductResponse>>());

        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.InstanceOf<ProductResponse>());
    }

    [Test]
    public async Task GetProductByIdNotFound()
    {
        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.GetProductByIdAsync("nonExistingId"))
            .ReturnsAsync((ProductResponse)null);

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.GetProductByIdAsync("nonExistingId");

        var notFoundResult = result.Result as NotFoundResult;

        Assert.That(notFoundResult, Is.Not.Null);
    }
    
    [Test]
    public async Task CreateProductCreated()
    {
        var mockRequest = new ProductCreateRequest
        {
            Name = "New Product",
            Type = Product.Type.BankAccount.ToString()
        };

        var mockResponse = new ProductResponse
        {
            Id = "newProductId",
            Name = "New Product",
            Type = "BankAccount" 
        };

        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.CreateProductAsync(mockRequest))
            .ReturnsAsync(mockResponse); 

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.CreateProductAsync(mockRequest);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null); 
        Assert.That(createdResult.ActionName, Is.EqualTo("GetProductByIdAsync")); 
        Assert.That(createdResult.RouteValues["id"], Is.EqualTo("newProductId")); 
        Assert.That(createdResult.Value, Is.InstanceOf<ProductResponse>()); 

        var product = createdResult.Value as ProductResponse;
        Assert.That(product.Id, Is.EqualTo("newProductId"));
        Assert.That(product.Name, Is.EqualTo("New Product"));
        Assert.That(product.Type, Is.EqualTo("BankAccount")); 
    }

    
    [Test]
    public async Task UpdateProductUpdated()
    {
        var productId = "existingProductId";

        var mockRequest = new ProductUpdateRequest
        {
            Name = "Updated Product",
            Type = Product.Type.BankAccount.ToString()
        };

        var mockResponse = new ProductResponse
        {
            Id = productId,
            Name = "Updated Product",
            Type = "BankAccount"
        };

        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.UpdateProductAsync(productId, mockRequest))
            .ReturnsAsync(mockResponse);

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.UpdateProductAsync(productId, mockRequest);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null); 
        Assert.That(createdResult.ActionName, Is.EqualTo("GetProductByIdAsync"));
        Assert.That(createdResult.RouteValues["id"], Is.EqualTo(productId)); 
        Assert.That(createdResult.Value, Is.InstanceOf<ProductResponse>()); 

        var product = createdResult.Value as ProductResponse;
        Assert.That(product.Id, Is.EqualTo(productId));
        Assert.That(product.Name, Is.EqualTo("Updated Product"));
        Assert.That(product.Type, Is.EqualTo("BankAccount")); 
    }
    
    [Test]
    public async Task UpdateProductNotfound()
    {
        var productId = "nonExistingProductId";

        var mockRequest = new ProductUpdateRequest
        {
            Name = "Updated Product",
            Type = "BankAccount" 
        };

        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.UpdateProductAsync(productId, mockRequest))
            .ReturnsAsync((ProductResponse)null);

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.UpdateProductAsync(productId, mockRequest);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>()); 
        mockService.Verify(service => service.UpdateProductAsync(productId, mockRequest), Times.Once);
    }
    
    [Test]
    public async Task DeleteProductDeleted()
    {
        var productId = "existingProductId";

        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.DeleteProductAsync(productId))
            .Returns(Task.FromResult(true));

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.DeleteProductAsync(productId);

        Assert.That(result, Is.InstanceOf<NoContentResult>());
        mockService.Verify(service => service.DeleteProductAsync(productId), Times.Once); 
    }
    
    [Test]
    public async Task DeleteProductNotFound()
    {
        var productId = "nonExistingProductId";

        var mockService = new Mock<IProductService>();
        mockService
            .Setup(service => service.DeleteProductAsync(productId))
            .ReturnsAsync(false); 

        var logger = NullLogger<ProductController>.Instance;
        var controller = new ProductController(mockService.Object, logger);

        var result = await controller.DeleteProductAsync(productId);

        Assert.That(result, Is.InstanceOf<NotFoundResult>()); 
        mockService.Verify(service => service.DeleteProductAsync(productId), Times.Once); 
    }
    
}
