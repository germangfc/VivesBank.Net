namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Generador de CVC (Código de Verificación de la Tarjeta de Crédito).
/// Esta clase genera un número aleatorio de 3 dígitos, que se utiliza como CVC para tarjetas de crédito.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class CvcGenerator : ICvcGenerator
{
    /// <summary>
    /// Genera un número aleatorio de 3 dígitos para el CVC.
    /// </summary>
    /// <returns>Un string que representa un número de 3 dígitos aleatorios (formato "DDD").</returns>
    public virtual string Generate()
    {
        var random = new Random();  // Instancia del generador de números aleatorios
        int randomNumber = random.Next(0, 1000);  // Genera un número aleatorio entre 0 y 999
        return randomNumber.ToString("D3");  // Formatea el número a 3 dígitos, añadiendo ceros a la izquierda si es necesario
    }
}
