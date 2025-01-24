using System.Text.RegularExpressions;

namespace VivesBankApi.Rest.Movimientos.Validators;

public class NumTarjetaValidator
{
        private static readonly Regex TarjetaRegex = new Regex("^[0-9]{16}$", RegexOptions.Compiled);

        public static bool ValidateTarjeta(string tarjeta)
        {
            if (string.IsNullOrEmpty(tarjeta) || tarjeta.Length != 16 || !TarjetaRegex.IsMatch(tarjeta))
            {
                return false; 
            }

            return ValidateLuhn(tarjeta);
        }

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


