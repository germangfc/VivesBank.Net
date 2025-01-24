using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

public class IbanValidator
{
    private static readonly Regex IbanRegex = new Regex("^[A-Z0-9]+$", RegexOptions.Compiled);
    public static bool ValidateIban(string iban)
    {
        if (string.IsNullOrEmpty(iban) || iban.Length < 15 || iban.Length > 34 || !IbanRegex.IsMatch(iban))
        {
            return false; 
        }

        var reorganizedIban = iban.Substring(4) + iban.Substring(0, 4);

        var numericIban = new System.Text.StringBuilder();
        foreach (var c in reorganizedIban)
        {
            if (char.IsDigit(c))
            {
                numericIban.Append(c);
            }
            else
            {
                numericIban.Append((c - 'A' + 10));
            }
        }

        return Modulo97(numericIban.ToString()) == 1;
    }

    private static int Modulo97(string numericIban)
    {
        var remainder = 0;
        for (var i = 0; i < numericIban.Length; i += 7)
        {
            var segment = remainder + numericIban.Substring(i, Math.Min(7, numericIban.Length - i));
            remainder = int.Parse(segment) % 97;
        }
        return remainder;
    }

}