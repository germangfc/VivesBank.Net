using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generators;

[TestFixture]
[TestOf(typeof(CvcGenerator))]
public class CvcGeneratorTest
{

    public CvcGenerator _cvcGenerator;


    [Test]
    public void GenerateCvc()
    {
        _cvcGenerator = new CvcGenerator();
        var cvc = _cvcGenerator.Generate();
        Assert.That(cvc.Length, Is.EqualTo(3));
        Assert.That(int.TryParse(cvc, out _), Is.True);
    }
}