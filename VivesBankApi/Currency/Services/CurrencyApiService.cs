using ApiFranfurkt.Properties.Currency.Exceptions;
using ApiFrankfurt.Configuration;
using Refit;

namespace ApiFranfurkt.Properties.Currency.Services
{
    /// <summary>
    /// Servicio que interactúa con la API de Frankfurter para obtener tasas de cambio.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class CurrencyApiService : ICurrencyApiService
    {
        private readonly ICurrencyApiService _apiClient;
        private readonly ILogger<CurrencyApiService> _logger;

        /// <summary>
        /// Constructor que inicializa el cliente API usando un HttpClient y un logger.
        /// </summary>
        /// <param name="apiClient">Cliente API utilizado para hacer las solicitudes.</param>
        /// <param name="logger">Logger para registrar los eventos.</param>
        public CurrencyApiService(ICurrencyApiService apiClient, ILogger<CurrencyApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Método para obtener las tasas de cambio más recientes desde la API.
        /// </summary>
        /// <param name="baseCurrency">Moneda base de la que se obtendrán las tasas.</param>
        /// <param name="symbols">Monedas objetivo para las que se obtendrán las tasas.</param>
        /// <returns>Respuesta de la API con las tasas de cambio.</returns>
        public async Task<ApiResponse<ExchangeRateResponse>> GetLatestRatesAsync(string baseCurrency, string symbols)
        {
            _logger.LogInformation($"Fetching exchange rates for BaseCurrency: {baseCurrency}, Symbols: {symbols}");

            // Hacer la llamada a la API externa
            var response = await _apiClient.GetLatestRatesAsync(baseCurrency, symbols);

            // Verificar si la llamada fue exitosa
            if (!response.IsSuccessStatusCode)
            {
                throw new CurrencyConnectionException(
                    $"Error connecting to API. Status code: {(int)response.StatusCode} ({response.StatusCode})");
            }

            // Validación del contenido de la respuesta.
            if (response.Content == null)
            {
                _logger.LogError("API response content is null.");
                throw new CurrencyEmptyResponseException();
            }

            _logger.LogInformation("Successfully fetched exchange rates.");
            return response;
        }
    }
}
