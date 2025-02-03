using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

/// <summary>
/// Validador para números de tarjeta (tarjetas de crédito o débito).
/// </summary>
/// <remarks>
/// Esta clase valida los números de tarjeta asegurándose de que tenga 16 dígitos numéricos y pase la comprobación del algoritmo de Luhn.
/// El algoritmo de Luhn se utiliza para validar tarjetas de crédito y débito.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class NumTarjetaValidator
{
    private static readonly Regex TarjetaRegex = new Regex("^[0-9]{16}$", RegexOptions.Compiled);

    /// <summary>
    /// Valida un número de tarjeta verificando el formato y el algoritmo de Luhn.
    /// </summary>
    /// <param name="tarjeta">El número de tarjeta a validar.</param>
    /// <returns>Devuelve <c>true</c> si el número de tarjeta es válido, de lo contrario devuelve <c>false</c>.</returns>
    /// <remarks>
    /// El número de tarjeta es validado en dos pasos:
    /// 1. Se comprueba que tenga exactamente 16 dígitos numéricos.
    /// 2. Se realiza la comprobación del número utilizando el algoritmo de Luhn, que es un algoritmo matemático utilizado para validar números de tarjeta.
    /// </remarks>
    public static bool ValidateTarjeta(string tarjeta)
    {
        if (string.IsNullOrEmpty(tarjeta) || tarjeta.Length != 16 || !TarjetaRegex.IsMatch(tarjeta))
        {
            return false; 
        }

        return ValidateLuhn(tarjeta);
    }

    /// <summary>
    /// Valida el número de tarjeta utilizando el algoritmo de Luhn.
    /// </summary>
    /// <param name="tarjeta">El número de tarjeta a validar.</param>
    /// <returns>Devuelve <c>true</c> si el número de tarjeta pasa la comprobación de Luhn, de lo contrario devuelve <c>false</c>.</returns>
    /// <remarks>
    /// El algoritmo de Luhn consiste en verificar la validez de un número de tarjeta de crédito o débito. 
    /// Se realiza una serie de operaciones matemáticas sobre los dígitos del número para determinar si es válido.
    /// </remarks>
    private static bool ValidateLuhn(string tarjeta)
    {
        var suma = 0;
        var duplicar = false;

        for (var i = tarjeta.Length - 1; i >= 0; i--)
        {
            var digit = tarjeta[i] - '0';

            if (duplicar)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit -= 9;
                }
            }

            suma += digit;
            duplicar = !duplicar;
        }

        return suma % 10 == 0;
    }
}



