using NUnit.Framework;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Validators;

[TestFixture]
[TestOf(typeof(IbanValidator))]
public class IbanValidatorTest
{
    [Test]
    public void ValidateIban_ValidIban_ReturnsTrue()
    {
        var validIban = "GB82WEST12345698765432"; // Ejemplo de IBAN válido
        ClassicAssert.IsTrue(IbanValidator.ValidateIban(validIban));
    }

    [Test]
    public void ValidateIban_TooShortIban_ReturnsFalse()
    {
        var shortIban = "GB82WEST123";
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(shortIban));
    }

    [Test]
    public void ValidateIban_TooLongIban_ReturnsFalse()
    {
        var longIban = "GB82WEST12345698765432123456789012345";
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(longIban));
    }

    [Test]
    public void ValidateIban_InvalidCharacters_ReturnsFalse()
    {
        var invalidIban = "GB82WEST12345@98765432";
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(invalidIban));
    }

    [Test]
    public void ValidateIban_NullOrEmptyIban_ReturnsFalse()
    {
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(null));
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(""));
    }

    [Test]
    public void ValidateIban_InvalidChecksum_ReturnsFalse()
    {
        var invalidChecksumIban = "GB82WEST12345698765431";
        ClassicAssert.IsFalse(IbanValidator.ValidateIban(invalidChecksumIban));
    }
}