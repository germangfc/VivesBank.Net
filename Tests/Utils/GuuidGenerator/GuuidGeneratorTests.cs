using NUnit.Framework.Legacy;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Utils.GuuidGenerator;

public class GuuidGeneratorTests
{
    [Test]
    public void GenerateHash_ShouldReturnValidBase64String()
    {
        // Act
        var hash = VivesBankApi.utils.GuuidGenerator.GuuidGenerator.GenerateHash();

        // Assert
        ClassicAssert.IsNotNull(hash);
        ClassicAssert.IsNotEmpty(hash);
        ClassicAssert.AreEqual(11, hash.Length); 
        ClassicAssert.IsTrue(hash.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'));
    }
}
