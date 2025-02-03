namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Interfaz que define el contrato para la generación de números de tarjetas de crédito.
/// Implementaciones de esta interfaz deben proporcionar un método para generar un número de tarjeta de crédito válido.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public interface INumberGenerator
{
    /// <summary>
    /// Genera un número de tarjeta de crédito aleatorio.
    /// </summary>
    /// <returns>Un número de tarjeta de crédito representado como un string.</returns>
    string GenerateCreditCardNumber();
}
