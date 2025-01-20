using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generators;

[TestFixture]
[TestOf(typeof(NumberGenerator))]
public class NumberGeneratorTest
{
    private NumberGenerator _numberGenerator;

    [SetUp]
    public void SetUp()
    {
        _numberGenerator = new NumberGenerator();
    }

    [Test]
    public void GenerateCreditCardNumber()
    {
        var number = _numberGenerator.GenerateCreditCardNumber();

        Assert.That(number.Length, Is.EqualTo(16));
        Assert.That(number, Does.Match(@"^\d+$"));
    }

    [Test]
    public void CalculateLuhnCheckDigit_ValidInput_ReturnsCorrectCheckDigit()
    {
        var cardNumber = new[] { 4, 5, 6, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2 };

        var checkDigit = _numberGenerator.CalculateLuhnCheckDigit(cardNumber);

        Assert.That(checkDigit, Is.EqualTo(2));
    }
}
