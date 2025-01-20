using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Validators;

namespace Tests.Rest.Product.Base.Validator;

[TestFixture]
[TestOf(typeof(ProductValidator))]
public class ProductValidatorTest
{
    private ProductValidator _productValidator;

    [Test]
    public void isValidProduct()
    {
        var product = new ProductCreateRequest { Name = "Test Product", Type = "BankAccount" };
        var isValid = ProductValidator.isValidProduct(product);
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void invalidProductName()
    {
        var product = new ProductCreateRequest { Type = "BankAccount" };
        var isValid = ProductValidator.isValidProduct(product);
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void invalidProductType()
    {
        var product = new ProductCreateRequest { Name = "Test Product" };
        var isValid = ProductValidator.isValidProduct(product);
        Assert.That(isValid, Is.False);
    }
    

}