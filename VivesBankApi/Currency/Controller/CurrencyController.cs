using ApiFranfurkt.Properties.Currency.Services;
using Microsoft.AspNetCore.Mvc;

using ApiFranfurkt.Properties.Currency.Services; // Servicio para obtener tasas de cambio de divisas
using Microsoft.AspNetCore.Mvc; // Espacio de nombres para la implementación de controladores en ASP.NET Core

using ApiFranfurkt.Properties.Currency.Services; // Servicio para obtener tasas de cambio de divisas
using Microsoft.AspNetCore.Mvc; // Espacio de nombres para la implementación de controladores en ASP.NET Core

namespace ApiFranfurkt.Properties.Currency.Controller
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con las tasas de cambio de divisas.
    /// Proporciona un endpoint para obtener las tasas de cambio más recientes.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    [ApiController]
    [Route("api/v1/currency")] // Define la ruta base para las peticiones del controlador
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyApiService _currencyApiService; // Servicio para interactuar con la API de divisas

        /// <summary>
        /// Constructor que inyecta la dependencia del servicio ICurrencyApiService.
        /// </summary>
        /// <param name="currencyApiService">Servicio que maneja las tasas de cambio de divisas</param>
        public CurrencyController(ICurrencyApiService currencyApiService)
        {
            _currencyApiService = currencyApiService; // Asigna el servicio inyectado
        }

        /// <summary>
        /// Obtiene las tasas de cambio más recientes.
        /// </summary>
        /// <param name="amount">Cantidad que se desea convertir (valor por defecto es 1)</param>
        /// <param name="baseCurrency">Moneda base desde la cual se va a realizar la conversión (valor por defecto es EUR)</param>
        /// <param name="symbols">Lista de monedas objetivo a las que se desea convertir (valor por defecto es null)</param>
        /// <returns>Respuesta con las tasas de cambio y la cantidad convertida</returns>
        [HttpGet("latest")] // Ruta para obtener las tasas de cambio más recientes
        public async Task<IActionResult> GetLatestRates(
            [FromQuery] string amount = "1", // Valor por defecto para la cantidad es "1"
            [FromQuery] string baseCurrency = "EUR", // Valor por defecto para la moneda base es "EUR"
            [FromQuery] string? symbols = null) // Parámetro opcional para las monedas de destino
        {
            // Validar y convertir el parámetro amount.
            if (!decimal.TryParse(amount, out var parsedAmount) || parsedAmount <= 0)
            {
                // Si la cantidad no es válida o es menor o igual a cero, devolver error 400 (Bad Request).
                return BadRequest("Invalid quantity. Value must be a positive number.");
            }

            // Validar que baseCurrency no sea nulo o vacío.
            if (string.IsNullOrWhiteSpace(baseCurrency))
            {
                // Si la moneda base es nula o vacía, devolver error 400 (Bad Request).
                return BadRequest("Invalid base currency. Please provide a valid currency code.");
            }

            // Validar el parámetro symbols si es proporcionado.
            if (!string.IsNullOrWhiteSpace(symbols))
            {
                // Separar los símbolos por comas y eliminar espacios en blanco.
                var symbolList = symbols.Split(',')
                    .Select(s => s.Trim())
                    .ToList();

                // Verificar si symbols contiene valores vacíos o inválidos.
                if (symbolList.Any(string.IsNullOrWhiteSpace))
                {
                    // Si alguno de los símbolos es inválido, devolver error 400 (Bad Request).
                    return BadRequest("Invalid symbol parameter. Please provide valid currency codes separated by commas.");
                }
            }

            // Si no se proporciona symbols, asignar una cadena vacía.
            var targetCurrencies = symbols ?? string.Empty;

            try
            {
                // Llamar al servicio para obtener las tasas de cambio más recientes.
                var apiResponse = await _currencyApiService.GetLatestRatesAsync(baseCurrency, targetCurrencies);
                
                // Obtener la respuesta de tasas de cambio.
                var exchangeRateResponse = apiResponse.Content;

                // Multiplicar las tasas de cambio por la cantidad deseada.
                if (exchangeRateResponse.Rates != null)
                {
                    // Para cada tasa de cambio, multiplicarla por la cantidad proporcionada.
                    foreach (var currency in exchangeRateResponse.Rates.Keys.ToList())
                    {
                        exchangeRateResponse.Rates[currency] *= (double)parsedAmount;
                    }
                }

                // Devolver la respuesta con las tasas de cambio actualizadas.
                return Ok(exchangeRateResponse);
            }
            catch (Exception ex)
            {
                // Si ocurre un error al obtener las tasas de cambio, devolver un error 500 (Internal Server Error).
                return StatusCode(500, $"Error getting exchange rates: {ex.Message}");
            }
        }
    }
}
