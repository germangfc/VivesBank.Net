using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Users.Validator;

namespace Tests.Rest.Users.Validator;

[TestFixture]
public class UserValidatorTests
{
    
    [Test]
    [TestCase("", false)]
    [TestCase("   ", false)]
    [TestCase("1234567Z", false)]    // 7 dígitos + letra (longitud 8)
    [TestCase("1234567890Z", false)] // 10 caracteres
    [TestCase("ABCD5678Z", false)]   // Letras en parte numérica
    [TestCase("123!5678Z", false)]   // Caracteres especiales
    [TestCase("12345678Z", true)]    // DNI válido (resto 14 -> Z)
    [TestCase("00000000T", true)]    // Caso límite mínimo (resto 0 -> T)
    [TestCase("00000023T", true)]    // 23 % 23 = 0 -> T
    [TestCase("00000022E", true)]    // 22 % 23 = 22 -> E
    [TestCase("12345678A", false)]   // Letra incorrecta
    [TestCase("12345678z", false)]   // Letra correcta en minúscula
    [TestCase("99999999R", true)]    // Máximo valor numérico (99999999 % 23 = 15 -> R)
    public void ValidateDni_ReturnsCorrectValidationResult(string dni, bool expectedResult)
    {
        // Act
        var result = UserValidator.ValidateDni(dni);

        // Assert
        ClassicAssert.AreEqual(expectedResult, result);
    }

    [Test]
    public void ValidateDni_ValidatesAllMod23Cases()
    {
        // Arrange
        var validLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
    
        for (int i = 0; i < 23; i++)
        {
            var numericPart = i; // Usamos i directamente
            var dni = $"{numericPart:D8}{validLetters[i % 23]}"; // Formato de 8 dígitos

            // Act
            var result = UserValidator.ValidateDni(dni);

            // Assert
            ClassicAssert.IsTrue(result, $"Failed for index {i} with DNI: {dni}");
        }
        
    }
}