using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

public class CifValidator
{
    private static readonly Regex CifRegex = new Regex("^[A-HJ-NP-SUVW][0-9]{7}[0-9A-J]$", RegexOptions.Compiled);

    public bool ValidateCif(string cif)
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
