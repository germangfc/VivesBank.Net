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
        try
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
                    $"Error connecting to API. Status code: {(int)response.StatusCode} ({response.StatusCode})"
                );
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
        catch (CurrencyConnectionException ex)
        {
            _logger.LogError(ex, "Connection exception occurred while fetching exchange rates.");
            throw;
        }
        catch (CurrencyEmptyResponseException ex)
        {
            _logger.LogError(ex, "Empty response exception occurred.");
            throw;
        }
        catch (Exception ex)
        {
            // Capturamos más detalles del error para entender mejor el problema
            _logger.LogError(ex, "Unexpected exception occurred while fetching exchange rates.");
            throw new CurrencyUnexpectedException("Error getting exchange rates.", ex);
        }
    }


    // Método para obtener las tasas de cambio más recientes con la cantidad especificada.
    
    public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency, string targetCurrencies,
        string amount)
    {
        _logger.LogInformation($"Fetching rates for {baseCurrency} with targets {targetCurrencies}");

        try
        {
            if (string.IsNullOrWhiteSpace(baseCurrency))
                throw new ArgumentException("Base currency cannot be null or empty.", nameof(baseCurrency));
            if (string.IsNullOrWhiteSpace(targetCurrencies))
                throw new ArgumentException("Target currencies cannot be null or empty.", nameof(targetCurrencies));

            var response = await _apiClient.GetLatestRatesAsync(baseCurrency, targetCurrencies);

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError($"API returned an unsuccessful status or null content: {response.StatusCode}");
                throw new CurrencyConnectionException($"Failed to connect to the currency API: {response.StatusCode}");
            }

            if (response.Content.Rates == null || response.Content.Rates.Count == 0)
            {
                _logger.LogError("API response contains no rates.");
                throw new CurrencyEmptyResponseException();
            }

            ConvertExchangeRates(response.Content, amount);
            _logger.LogInformation("Successfully fetched and processed rates.");

            return response.Content;
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogError("The requested resource was not found on the remote server.");
            throw new CurrencyEmptyResponseException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching exchange rates.");
            throw new CurrencyUnexpectedException("An unexpected error occurred while retrieving the exchange rates.",
                ex);
        }
    }


    public void ConvertExchangeRates(ExchangeRateResponse response, string amount)
    {
        _logger.LogInformation($"Converting exchange rates with Amount: {amount}");

        if (!double.TryParse(amount, out var parsedAmount))
        {
            _logger.LogError($"Invalid amount provided: {amount}");
            throw new CurrencyUnexpectedException("The amount provided is not valid.");
        }

        if (response.Rates == null || response.Rates.Count == 0)
        {
            _logger.LogError("Exchange rates are empty or null.");
            throw new CurrencyEmptyResponseException();
        }

        var convertedRates = new Dictionary<string, double>();

        foreach (var rate in response.Rates)
        {
            convertedRates[rate.Key] = rate.Value * parsedAmount;
        }

        response.Rates = convertedRates;
        _logger.LogInformation("Exchange rates successfully converted.");
    }
}
