using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Users.Validator;

/// <summary>
/// Proporciona métodos de validación para los usuarios, como la validación del DNI.
/// </summary>
public class UserValidator
{
    /// <summary>
    /// Valida un DNI español para asegurarse de que cumple con el formato y los cálculos correctos.
    /// </summary>
    /// <param name="dni">El DNI a validar.</param>
    /// <returns>Devuelve true si el DNI es válido, de lo contrario false.</returns>
    /// <remarks>
    /// El DNI español consta de 8 dígitos seguidos de una letra. Los primeros 8 caracteres son numéricos,
    /// y el noveno carácter es una letra calculada en base al número resultante de los primeros 8 dígitos
    /// mediante una fórmula mod 23.
    /// </remarks>
    public static bool ValidateDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni) || dni.Length != 9)
            return false;

        var digitsPart = dni.Substring(0, 8);
        var letterPart = dni[8];
        
        if (!int.TryParse(digitsPart, out int numericPart))
            return false;

        var validLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
        var expectedLetter = validLetters[numericPart % 23];

        return letterPart == expectedLetter;
    }
}
