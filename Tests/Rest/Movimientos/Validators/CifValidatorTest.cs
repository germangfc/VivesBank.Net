using NUnit.Framework;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Validators;

namespace Tests.Rest.Movimientos.Validators;

[TestFixture]
[TestOf(typeof(CifValidator))]
public class CifValidatorTest
{
    [Test]
    public void ValidateCif_ValidCifWithNumberControl_ReturnsTrue()
    {
        var validCif = "A12345674"; // Número de control correcto calculado
        ClassicAssert.IsTrue(CifValidator.ValidateCif(validCif));
    }

    [Test]
    public void ValidateCif_ValidCifWithLetterControl_ReturnsTrue()
    {
        var validCif = "R8061138G"; // Letra de control correcta calculada
        ClassicAssert.IsTrue(CifValidator.ValidateCif(validCif));
    }

    [Test]
    public void ValidateCif_InvalidCifLength_ReturnsFalse()
    {
        var invalidCif = "A1234567";
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }

    [Test]
    public void ValidateCif_InvalidCifFormat_ReturnsFalse()
    {
        var invalidCif = "X12345678";
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }

    [Test]
    public void ValidateCif_EmptyString_ReturnsFalse()
    {
        var invalidCif = "";
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }

    [Test]
    public void ValidateCif_NullString_ReturnsFalse()
    {
        string invalidCif = null;
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }

    [Test]
    public void ValidateCif_InvalidNumberControl_ReturnsFalse()
    {
        var invalidCif = "A12345679"; // Número de control incorrecto
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }

    [Test]
    public void ValidateCif_InvalidLetterControl_ReturnsFalse()
    {
        var invalidCif = "P1234567Z"; // Letra de control incorrecta 
        ClassicAssert.IsFalse(CifValidator.ValidateCif(invalidCif));
    }
    
    [Test]
    public void ValidateCif_ValidCifWithLetterControlDiferent_ReturnsTrue()
    {
        var validCif = "C26347872"; // Número de control correcto calculado
        ClassicAssert.IsTrue(CifValidator.ValidateCif(validCif));
    }
}