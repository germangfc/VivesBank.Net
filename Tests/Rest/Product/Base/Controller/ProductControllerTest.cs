using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using VivesBankApi.Rest.Product.Base.Controller;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Models;
using VivesBankApi.Rest.Product.Service;


[TestFixture]
[TestOf(typeof(ProductController))]
public class ProductControllerTests
{
    private Mock<IProductService> _productService;
    private Mock<ILogger<ProductController>> _logger;
    private ProductController _controller;
    
    //Products
    private ProductCreateRequest _productCreateRequest;
    private ProductUpdateRequest _productUpdateRequest;
    private ProductResponse _productResponse1;
    private ProductResponse _productResponse2;
    private ProductResponse _productResponse3;
    private ProductResponse _productResponse4;
    private List<ProductResponse> _products;

    [SetUp]
    public void SetUp()
    {
        _productService = new Mock<IProductService>();
        _logger = new Mock<ILogger<ProductController>>();
        _controller = new ProductController(_productService.Object, _logger.Object);

        _productCreateRequest = new ProductCreateRequest
        {
            Name = "ProductCreateName",
            Type = Product.Type.BankAccount.ToString()
        };

        _productUpdateRequest = new ProductUpdateRequest
        {
            Name = "ProductUpdateName",
            Type = Product.Type.BankAccount.ToString()
        };

        _productResponse1 = new ProductResponse
        {
            Id = "1",
            Name = "Product 1",
            Type = "CreditCard"
        };
        
        _productResponse2 = new ProductResponse
        {
            Id = "2",
            Name = "Product 2",
            Type = "BankAccount"
        };
        
        _productResponse3 = new ProductResponse
        {
            Id = "3",
            Name = "Product 3",
            Type = "BankAccount"
        };
        
        _productResponse4 = new ProductResponse
        {
            Id = "4",
            Name = "Product 4",
            Type = "CreditCard"
        };


        _products = new List<ProductResponse>
        {
            _productResponse1, _productResponse2, _productResponse3
        };
    }
    
    
    [Test]
    public async Task GetAllProductsAsync()
    {
        // Arrange
        _productService
            .Setup(service => service.GetAllProductsAsync())
            .ReturnsAsync(_products);

        // Act
        var result = await _controller.GetAllProductsAsync();
        var okResult = result.Result as OkObjectResult;
        var responseProducts = okResult?.Value as List<ProductResponse>;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ActionResult<List<ProductResponse>>>());
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
            Assert.That(responseProducts, Is.Not.Null);
            Assert.That(responseProducts, Is.InstanceOf<List<ProductResponse>>());
            Assert.That(responseProducts!.Count, Is.EqualTo(_products.Count));
            Assert.That(responseProducts, Is.EqualTo(_products));
        });
    }

    
    [Test]
    public async Task GetProductById()
    {
        // Arrange
        var productId = "3";
        _productService
            .Setup(service => service.GetProductByIdAsync(productId))
            .ReturnsAsync(_productResponse3);

        // Act
        var result = await _controller.GetProductByIdAsync(productId);
        var okResult = result.Result as OkObjectResult;
        var product = okResult?.Value as ProductResponse;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ActionResult<ProductResponse>>());
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
            Assert.That(product, Is.Not.Null);
            Assert.That(product!.Id, Is.EqualTo(_productResponse3.Id));
            Assert.That(product.Name, Is.EqualTo(_productResponse3.Name));
            Assert.That(product.Type, Is.EqualTo(_productResponse3.Type));
        });
    }
    
    
    [Test]
    public async Task GetProductById_NotFound()
    {
        // Arrange
        var productId = "999";
        _productService
            .Setup(service => service.GetProductByIdAsync(productId))
            .ReturnsAsync((ProductResponse)null);

        // Act
        var result = await _controller.GetProductByIdAsync(productId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ActionResult<ProductResponse>>());
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        
            var notFoundResult = result.Result as NotFoundResult;
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
        });
    }

    
    [Test]
    public async Task CreateProduct()
    {
        // Arrange
        var mockCreateRequest = _productCreateRequest;
        var mockResponse = _productResponse4;

        _productService
            .Setup(service => service.CreateProductAsync(mockCreateRequest))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.CreateProductAsync(mockCreateRequest);
        var createdResult = result.Result as CreatedAtActionResult;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ActionResult<ProductResponse>>());
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(ProductController.GetProductByIdAsync)));
            Assert.That(createdResult.RouteValues?["id"], Is.EqualTo(mockResponse.Id));
            Assert.That(createdResult.Value, Is.InstanceOf<ProductResponse>());
        });

        var product = createdResult.Value as ProductResponse;
        Assert.Multiple(() =>
        {
            Assert.That(product!.Id, Is.EqualTo(mockResponse.Id));
            Assert.That(product.Name, Is.EqualTo(mockResponse.Name));
            Assert.That(product.Type, Is.EqualTo(mockResponse.Type));
        });
    }
    
    
    [Test]
    public async Task UpdateProductAsync()
    {
        // Arrange
        var productId = "1";
        var mockRequest = _productUpdateRequest; 
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

        // Act
        var result = await controller.UpdateProductAsync(productId, mockRequest);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;

        Assert.Multiple(() =>
        {
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult.ActionName, Is.EqualTo("GetProductByIdAsync"));
            Assert.That(createdResult.RouteValues["id"], Is.EqualTo(productId));
            
            var product = createdResult.Value as ProductResponse;
            Assert.That(product, Is.Not.Null);
            Assert.That(product.Id, Is.EqualTo(productId));
            Assert.That(product.Name, Is.EqualTo("Updated Product"));
            Assert.That(product.Type, Is.EqualTo("BankAccount"));
        });
        
        mockService.Verify(service => service.UpdateProductAsync(productId, mockRequest), Times.Once);
    }
    
    
    [Test]
    public async Task UpdateProductAsync_NotFound()
    {
        // Arrange
        var productId = "999";
        var mockRequest = new ProductUpdateRequest
        {
            Name = "Updated Product",
            Type = "CreditCard"
        };
        
        _productService.Setup(service => service.UpdateProductAsync(productId, mockRequest))
            .ReturnsAsync((ProductResponse)null);

        var controller = new ProductController(_productService.Object, NullLogger<ProductController>.Instance);

        // Act
        var result = await controller.UpdateProductAsync(productId, mockRequest);

        // Assert
        var notFoundResult = result.Result as NotFoundResult;
        Assert.That(notFoundResult?.StatusCode, Is.EqualTo(404)); 

        _productService.Verify(service => service.UpdateProductAsync(productId, mockRequest), Times.Once);
    }

    
    [Test]
    public async Task DeleteProductAsync()
    {
        // Arrange
        var productId = "1";
        _productService.Setup(service => service.DeleteProductAsync(productId))
            .ReturnsAsync(true);

        var controller = new ProductController(_productService.Object, NullLogger<ProductController>.Instance);

        // Act
        var result = await controller.DeleteProductAsync(productId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _productService.Verify(service => service.DeleteProductAsync(productId), Times.Once);
    }

    
    [Test]
    public async Task DeleteProductAsync_NotFound()
    {
        // Arrange
        var productId = "999";
        _productService.Setup(service => service.DeleteProductAsync(productId))
            .ReturnsAsync(false);

        var controller = new ProductController(_productService.Object, NullLogger<ProductController>.Instance);

        // Act
        var result = await controller.DeleteProductAsync(productId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundResult>());
        _productService.Verify(service => service.DeleteProductAsync(productId), Times.Once);
    }
}
