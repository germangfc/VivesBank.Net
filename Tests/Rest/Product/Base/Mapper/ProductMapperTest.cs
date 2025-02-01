using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.Base.Mapper;

namespace Tests.Rest.Product.Base.Mapper;

using System;
using NUnit.Framework;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Models;

[TestFixture]
public class ProductMapperTests
{
    [Test]
    public void ToDtoResponse_ValidProduct_ReturnsProductResponse()
    {
        // Arrange
        var product = new Product("TestProduct", Product.Type.CreditCard)
        {
            Id = "AnId",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var response = product.ToDtoResponse();

        // Assert
        ClassicAssert.AreEqual(product.Id, response.Id);
        ClassicAssert.AreEqual(product.Name, response.Name);
        ClassicAssert.AreEqual(product.ProductType.ToString(), response.Type);
        ClassicAssert.AreEqual(product.CreatedAt.ToString(), response.CreatedAt);
        ClassicAssert.AreEqual(product.UpdatedAt.ToString(), response.UpdatedAt);
    }

    [Test]
    public void FromDtoRequest_ValidRequest_ReturnsProduct()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "TestProduct",
            Type = "CreditCard"
        };

        // Act
        var product = request.FromDtoRequest();

        // Assert
        ClassicAssert.AreEqual(request.Name, product.Name);
        ClassicAssert.AreEqual(Product.Type.CreditCard, product.ProductType);
        ClassicAssert.IsFalse(product.IsDeleted);
    }

    [Test]
    public void FromDtoRequest_InvalidType_ThrowsProductInvalidTypeException()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "TestProduct",
            Type = "InvalidType"
        };

        // Act & Assert
        var exception = Assert.Throws<ProductException.ProductInvalidTypeException>(() => request.FromDtoRequest());
        ClassicAssert.IsTrue(exception.Message.Contains("Invalid Type"));
    }

    [Test]
    public void FromDtoRequest_NullOrEmptyType_ThrowsProductInvalidTypeException()
    {
        // Arrange
        var request = new ProductCreateRequest
        {
            Name = "TestProduct",
            Type = "  "
        };

        // Act & Assert
        var exception = Assert.Throws<ProductException.ProductInvalidTypeException>(() => request.FromDtoRequest());
        ClassicAssert.IsTrue(exception.Message.Contains("The Type field is required"));
    }
}
