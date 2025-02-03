namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Generador de números de tarjetas de crédito.
/// Esta clase genera un número de tarjeta de crédito válido de 16 dígitos siguiendo el algoritmo de Luhn.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class NumberGenerator : INumberGenerator
{
    /// <summary>
    /// Genera un número de tarjeta de crédito aleatorio de 16 dígitos.
    /// El número generado es válido según el algoritmo de Luhn.
    /// </summary>
    /// <returns>Un número de tarjeta de crédito representado como un string.</returns>
    public virtual string GenerateCreditCardNumber()
    {
        var random = new Random();  // Instancia del generador de números aleatorios
        int[] cardNumber = new int[16];  // Array para almacenar los 16 dígitos de la tarjeta

        // Genera los primeros 15 dígitos de la tarjeta de crédito
        for (int i = 0; i < 15; i++)
        {
            cardNumber[i] = random.Next(0, 10);
        }

        // Calcula el último dígito usando el algoritmo de Luhn
        cardNumber[15] = checkNumber(cardNumber);

        // Convierte el array de dígitos en un string y lo devuelve
        return string.Join(string.Empty, cardNumber);
    }

    /// <summary>
    /// Calcula el último dígito del número de tarjeta de crédito usando el algoritmo de Luhn.
    /// Este dígito es necesario para que el número de tarjeta sea válido.
    /// </summary>
    /// <param name="cardNumber">El array de los primeros 15 dígitos de la tarjeta de crédito.</param>
    /// <returns>El último dígito necesario para que el número de tarjeta sea válido según el algoritmo de Luhn.</returns>
    public int checkNumber(int[] cardNumber)
    {
        int sum = 0;

        // Recorre los primeros 15 dígitos para calcular la suma
        for (int i = 0; i < 15; i++)
        {
            int digit = cardNumber[i];

            // Si el índice es par (considerando el índice desde 0), duplica el valor del dígito
            if (i % 2 == 0)
            {
                digit *= 2;
                if (digit > 9)  // Si el dígito es mayor que 9, resta 9
                {
                    digit -= 9;
                }
            }

            sum += digit;  // Suma el dígito procesado al total
        }

        // Calcula el dígito de control y lo devuelve
        return (10 - (sum % 10)) % 10;
    }
}
