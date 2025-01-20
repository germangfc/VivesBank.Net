using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generators;

[TestFixture]
[TestOf(typeof(ExpirationDateGenerator))]
public class ExpirationDateGeneratorTest
{
    private ExpirationDateGenerator _expirationDateGenerator;

    [Test]
    public void GenerateRandomDate()
    {
        _expirationDateGenerator = new ExpirationDateGenerator();
        var date = _expirationDateGenerator.GenerateRandomDate();

        Assert.That(date.Year, Is.LessThan(DateTime.Today.Year + 6));
    }
}