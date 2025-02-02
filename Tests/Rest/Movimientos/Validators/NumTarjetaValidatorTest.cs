using NUnit.Framework;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Validators;

[TestFixture]
[TestOf(typeof(NumTarjetaValidator))]
public class NumTarjetaValidatorTest
{
    [Test]
    public void ValidateTarjeta_ValidTarjeta_ReturnsTrue()
    {
        var validTarjeta = "1234567812345670"; // Ejemplo de tarjeta válida
        ClassicAssert.IsTrue(NumTarjetaValidator.ValidateTarjeta(validTarjeta));
    }

    [Test]
    public void ValidateTarjeta_InvalidLength_ReturnsFalse()
    {
        var invalidTarjeta = "12345678"; // Tarjeta con longitud incorrecta
        ClassicAssert.IsFalse(NumTarjetaValidator.ValidateTarjeta(invalidTarjeta));
    }

    [Test]
    public void ValidateTarjeta_InvalidFormat_ReturnsFalse()
    {
        var invalidTarjeta = "1234-5678-1234-5670"; // Tarjeta con formato incorrecto (con guiones)
        ClassicAssert.IsFalse(NumTarjetaValidator.ValidateTarjeta(invalidTarjeta));
    }

    [Test]
    public void ValidateTarjeta_InvalidLuhn_ReturnsFalse()
    {
        var invalidTarjeta = "1234567812345671"; // Tarjeta válida en formato, pero no pasa la validación de Luhn
        ClassicAssert.IsFalse(NumTarjetaValidator.ValidateTarjeta(invalidTarjeta));
    }

    [Test]
    public void ValidateTarjeta_NullTarjeta_ReturnsFalse()
    {
        string nullTarjeta = null; // Tarjeta nula
        ClassicAssert.IsFalse(NumTarjetaValidator.ValidateTarjeta(nullTarjeta));
    }

    [Test]
    public void ValidateTarjeta_EmptyTarjeta_ReturnsFalse()
    {
        var emptyTarjeta = ""; // Tarjeta vacía
        ClassicAssert.IsFalse(NumTarjetaValidator.ValidateTarjeta(emptyTarjeta));
    }
}