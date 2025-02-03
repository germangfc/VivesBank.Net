using ApiFrankfurt.Configuration;
using Refit;

namespace ApiFranfurkt.Properties.Currency.Services
{
    /// <summary>
    /// Interfaz para definir el cliente Refit utilizado para interactuar con la API de tasas de cambio de Frankfurter.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public interface ICurrencyApiService
    {
        /// <summary>
        /// Método para obtener las tasas de cambio más recientes desde la API.
        /// </summary>
        /// <param name="baseCurrency">Moneda base de la que se obtendrán las tasas.</param>
        /// <param name="targetCurrencies">Monedas objetivo para las que se obtendrán las tasas.</param>
        /// <returns>Respuesta de la API con las tasas de cambio.</returns>
        [Get("/latest")]
        Task<ApiResponse<ExchangeRateResponse>> GetLatestRatesAsync(
            [AliasAs("base")] string baseCurrency,
            [AliasAs("symbols")] string targetCurrencies
        );
    }
}
