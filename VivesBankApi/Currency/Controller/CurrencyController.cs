using ApiFranfurkt.Properties.Currency.Exceptions;
using ApiFranfurkt.Properties.Currency.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiFranfurkt.Properties.Currency.Controller;

[ApiController]
[Route("api/v1/currency")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyApiService _currencyApiService;

    public CurrencyController(ICurrencyApiService currencyApiService)
    {
        _currencyApiService = currencyApiService;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestRates(
        [FromQuery] string amount = "1",
        [FromQuery] string baseCurrency = "EUR",
        [FromQuery] string? symbols = null)
    {
        // Validar y convertir el parámetro amount.
        if (!decimal.TryParse(amount, out var parsedAmount) || parsedAmount <= 0)
        {
            return BadRequest("Invalid amount. The value must be a positive number.");
        }

        // Validar que baseCurrency no sea nulo o vacío.
        if (string.IsNullOrWhiteSpace(baseCurrency))
        {
            return BadRequest("Invalid base currency. Please provide a valid currency code.");
        }
        
        if (!string.IsNullOrWhiteSpace(symbols))
        {
            var symbolList = symbols.Split(',')
                .Select(s => s.Trim())
                .ToList();

            // Verificar si `symbols` contiene valores vacíos o inválidos.
            if (symbolList.Any(string.IsNullOrWhiteSpace))
            {
                return BadRequest("Invalid symbols parameter. Please provide valid currency codes separated by commas.");
            }
        }
        
        var targetCurrencies = symbols ?? string.Empty;

        // Llamar al servicio para obtener las tasas de cambio.
        var apiResponse = await _currencyApiService.GetLatestRatesAsync(baseCurrency, targetCurrencies);
        
        // Multiplicar las tasas de cambio por la cantidad deseada.
        var exchangeRateResponse = apiResponse.Content;

        // Ejemplo: Supongamos que las tasas de cambio están en un diccionario.
        if (exchangeRateResponse.Rates != null)
        { 
            foreach (var currency in exchangeRateResponse.Rates.Keys.ToList())
            {
                exchangeRateResponse.Rates[currency] *= (double)parsedAmount;
            }
        }

        return Ok(exchangeRateResponse);
    }
}