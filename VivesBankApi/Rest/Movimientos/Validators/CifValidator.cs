using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

/// <summary>
/// Validador para el CIF (Código de Identificación Fiscal) de empresas en España.
/// </summary>
/// <remarks>
/// Esta clase se encarga de verificar la validez de un CIF español, realizando la comprobación de su formato y
/// calculando el dígito de control según las reglas oficiales.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class CifValidator
{
    private static readonly Regex CifRegex = new Regex("^[A-HJ-NP-SUVW][0-9]{7}[0-9A-J]$", RegexOptions.Compiled);

    /// <summary>
    /// Valida un CIF español.
    /// </summary>
    /// <param name="cif">El CIF a validar.</param>
    /// <returns>Devuelve <c>true</c> si el CIF es válido, de lo contrario devuelve <c>false</c>.</returns>
    /// <remarks>
    /// Este método comprueba si el CIF cumple con el formato correcto y realiza el cálculo del dígito de control
    /// según el tipo de letra inicial del CIF (A-HJ-NP-SUVW).
    /// </remarks>
    public static bool ValidateCif(string cif)
    {
        if (string.IsNullOrEmpty(cif) || !CifRegex.IsMatch(cif) || cif.Length != 9)
        {
            return false;
        }

        var letraInicial = cif[0]; 
        var numeros = cif.Substring(1, 7);
        var digitoControl = cif[8]; 

        var sumaPares = 0;
        for (var i = 1; i < numeros.Length; i += 2)
        {
            sumaPares += (int)char.GetNumericValue(numeros[i]);
        }

        var sumaImpares = 0;
        for (var i = 0; i < numeros.Length; i += 2)
        {
            var doble = (int)char.GetNumericValue(numeros[i]) * 2;
            sumaImpares += doble / 10 + doble % 10;
        }

        var sumaTotal = sumaPares + sumaImpares; 
        var unidadControl = (10 - (sumaTotal % 10)) % 10;

        if ("ABEH".IndexOf(letraInicial) != -1)
        {
            return (int)char.GetNumericValue(digitoControl) == unidadControl;
        }
        else if ("KPQRSNW".IndexOf(letraInicial) != -1)
        {
            var letraControl = "JABCDEFGHI"[unidadControl];
            return digitoControl == letraControl;
        }
        else
        {
            return (int)char.GetNumericValue(digitoControl) == unidadControl ||
                   digitoControl == "JABCDEFGHI"[unidadControl];
        }
    }
}

