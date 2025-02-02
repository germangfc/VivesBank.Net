using System.Security.Cryptography;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.CreditCard.Generators;

namespace Tests.Rest.Product.CreditCard.Generator;

public class CvcGeneratorTests
{
    [Test]
    public void Generate_ShouldReturnThreeDigitString()
    {
        var generator = new CvcGenerator();
        
        var result = generator.Generate();

        ClassicAssert.True(result.Length == 3, $"Expected a 3-digit number, but got {result.Length} digits.");
        ClassicAssert.True(int.TryParse(result, out int number), $"Expected a valid integer, but got {result}");
        ClassicAssert.True(number >= 0 && number <= 999, $"Expected number between 000 and 999, but got {number}");
    }
}