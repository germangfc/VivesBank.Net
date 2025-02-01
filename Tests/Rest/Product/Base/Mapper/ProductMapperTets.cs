using NUnit.Framework;
using System;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Mapper;
using VivesBankApi.Rest.Product.Base.Models;

[TestFixture]
public class ProductMapperTests
{
    private Product product;
    private ProductCreateRequest validCreateRequest;
    private ProductCreateRequest invalidCreateRequest;
    private ProductCreateRequest emptyTypeRequest;

    [SetUp]
    public void SetUp()
    {
        // Inicializar los objetos necesarios para los tests
        product = new Product("Test Product", Product.Type.BankAccount)
        {
            Id = "123",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        validCreateRequest = new ProductCreateRequest
        {
            Name = "New Product",
            Type = "BankAccount"
        };

        invalidCreateRequest = new ProductCreateRequest
        {
            Name = "Invalid Product",
            Type = "InvalidType"
        };

        emptyTypeRequest = new ProductCreateRequest
        {
            Name = "Invalid Product",
            Type = ""  
        };
    }

    [Test]
    public void ToDtoResponse_ValidProduct_ReturnsProductResponse()
    {
        // Act
        var result = product.ToDtoResponse();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(product.Id));
        Assert.That(result.Name, Is.EqualTo(product.Name));
        Assert.That(result.Type, Is.EqualTo(product.ProductType.ToString()));
        Assert.That(result.CreatedAt, Is.EqualTo(product.CreatedAt.ToString()));
        Assert.That(result.UpdatedAt, Is.EqualTo(product.UpdatedAt.ToString()));
    }

    [Test]
    public void FromDtoRequest_ValidRequest_ReturnsProduct()
    {
        // Act
        var result = validCreateRequest.FromDtoRequest();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(validCreateRequest.Name));
        Assert.That(result.ProductType, Is.EqualTo(Product.Type.BankAccount));
        Assert.That(result.IsDeleted, Is.False);
    }

    [Test]
    public void FromDtoRequest_InvalidType_ThrowsProductInvalidTypeException()
    {
        // Act & Assert
        var ex = Assert.Throws<ProductException.ProductInvalidTypeException>(() => invalidCreateRequest.FromDtoRequest());
        Assert.That(ex.Message, Is.EqualTo("Invalid product type: Invalid Type: InvalidType. Valid values are: BankAccount, CreditCard"));
    }

    [Test]
    public void FromDtoRequest_EmptyType_ThrowsProductInvalidTypeException()
    {
        // Act & Assert
        var ex = Assert.Throws<ProductException.ProductInvalidTypeException>(() => emptyTypeRequest.FromDtoRequest());
        Assert.That(ex.Message, Is.EqualTo("Invalid product type: The Type field is required and cannot be null or empty."));
    }
    
    [Test]
    public void Type_Get_ReturnsCorrectValue()
    {
        // Arrange
        var updateRequest = new ProductUpdateRequest
        {
            Type = "BankAccount"
        };

        // Act
        var type = updateRequest.Type;

        // Assert
        Assert.That(type, Is.EqualTo("BankAccount"));
    }
}
