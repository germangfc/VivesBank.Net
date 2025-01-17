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
        try
        {
            var targetCurrencies = symbols ?? string.Empty;

            var apiResponse = await _currencyApiService.GetLatestRatesAsync(baseCurrency, targetCurrencies);

            if (!apiResponse.IsSuccessStatusCode || apiResponse.Content == null)
            {
                return NotFound("Exchange rates not found for the given parameters.");
            }

            var exchangeRateResponse = apiResponse.Content;

            return Ok(exchangeRateResponse);
        }
        catch (CurrencyEmptyResponseException ex)
        {
            return NotFound(ex.Message);
        }
        catch (CurrencyUnexpectedException ex)
        {
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
        }
    }
}