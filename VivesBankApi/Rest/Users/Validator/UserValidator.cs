using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Users.Validator;

public class UserValidator
{
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