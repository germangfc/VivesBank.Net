using ApiFranfurkt.Properties.Currency.Exceptions;
using ApiFrankfurt.Configuration;

namespace ApiFranfurkt.Properties.Currency.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Refit;

public class CurrencyApiService : ICurrencyApiService
{
    private readonly ICurrencyApiService _apiClient;
    private readonly ILogger<CurrencyApiService> _logger;

    // Constructor que inicializa el cliente API usando un HttpClient y un logger.
    
    public CurrencyApiService(ICurrencyApiService apiClient, ILogger<CurrencyApiService> logger)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Método para obtener las tasas de cambio más recientes desde la API.
    public async Task<ApiResponse<ExchangeRateResponse>> GetLatestRatesAsync(string baseCurrency, string symbols)
    {
      
        _logger.LogInformation($"Fetching exchange rates for BaseCurrency: {baseCurrency}, Symbols: {symbols}");

        // Hacer la llamada a la API externa
        var response = await _apiClient.GetLatestRatesAsync(baseCurrency, symbols);

        // Verificar si la llamada fue exitosa
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"API returned non-success status code: {response.StatusCode}");
            if (response.Content != null)
            {
                // Convertimos el contenido de la respuesta a String si es necesario para el log
                var errorContent = response.Content.ToString();
                _logger.LogError($"API Response Content: {errorContent}");
            }

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
