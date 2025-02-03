using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

/// <summary>
/// Validador para el IBAN (International Bank Account Number).
/// </summary>
/// <remarks>
/// Esta clase valida un IBAN asegurándose de que tenga el formato adecuado y de que pase el algoritmo de comprobación
/// de validez basado en el módulo 97. El IBAN puede tener entre 15 y 34 caracteres.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class IbanValidator
{
    private static readonly Regex IbanRegex = new Regex("^[A-Z0-9]+$", RegexOptions.Compiled);

    /// <summary>
    /// Valida un IBAN (International Bank Account Number).
    /// </summary>
    /// <param name="iban">El IBAN a validar.</param>
    /// <returns>Devuelve <c>true</c> si el IBAN es válido, de lo contrario devuelve <c>false</c>.</returns>
    /// <remarks>
    /// El IBAN es validado en dos pasos:
    /// 1. Se comprueba que tenga el formato correcto (solo caracteres alfanuméricos y longitud entre 15 y 34).
    /// 2. Se realiza la comprobación del dígito de control utilizando el algoritmo de módulo 97.
    /// </remarks>
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

    /// <summary>
    /// Calcula el resultado del algoritmo de módulo 97 sobre una cadena numérica.
    /// </summary>
    /// <param name="numericIban">El IBAN reorganizado en formato numérico.</param>
    /// <returns>El residuo de la operación módulo 97.</returns>
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