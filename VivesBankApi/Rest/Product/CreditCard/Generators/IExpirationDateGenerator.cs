namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Interfaz que define el contrato para la generación de fechas de expiración de tarjetas de crédito.
/// Implementaciones de esta interfaz deben proporcionar un método para generar una fecha de expiración aleatoria.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public interface IExpirationDateGenerator
{
    /// <summary>
    /// Genera una fecha aleatoria de expiración dentro de un rango de tiempo predeterminado.
    /// </summary>
    /// <returns>Una fecha aleatoria de expiración representada por un objeto <see cref="DateOnly"/>.</returns>
    DateOnly GenerateRandomDate();
}
