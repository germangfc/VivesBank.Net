namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Interfaz que define el contrato para la generación de CVC (Código de Verificación de la Tarjeta de Crédito).
/// Implementaciones de esta interfaz deben proporcionar un método para generar un CVC aleatorio de 3 dígitos.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public interface ICvcGenerator
{
     /// <summary>
     /// Genera un CVC aleatorio de 3 dígitos para una tarjeta de crédito.
     /// </summary>
     /// <returns>Un string que representa un CVC aleatorio de 3 dígitos (formato "DDD").</returns>
     string Generate();
}
