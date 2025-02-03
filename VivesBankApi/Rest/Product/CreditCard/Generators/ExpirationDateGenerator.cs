namespace VivesBankApi.Rest.Product.CreditCard.Generators;

/// <summary>
/// Generador de fechas de expiración para tarjetas de crédito.
/// Esta clase genera una fecha aleatoria de expiración dentro de un rango de 5 años desde la fecha actual.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class ExpirationDateGenerator : IExpirationDateGenerator
{
    /// <summary>
    /// Genera una fecha aleatoria de expiración dentro de los próximos 5 años desde la fecha actual.
    /// </summary>
    /// <returns>Una fecha de expiración aleatoria representada por un objeto <see cref="DateOnly"/>.</returns>
    public virtual DateOnly GenerateRandomDate()
    {
        var random = new Random();  // Instancia del generador de números aleatorios
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);  // Obtiene la fecha de hoy
        DateOnly futureDate = today.AddYears(5);  // Fecha 5 años en el futuro

        // Calcula el rango de días entre la fecha actual y la fecha futura (5 años)
        int range = (futureDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;

        // Devuelve una fecha aleatoria dentro del rango de días calculado
        return today.AddDays(random.Next(range));
    }
}
