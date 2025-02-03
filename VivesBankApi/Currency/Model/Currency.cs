using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiFranfurkt.Properties.Currency
{
    /// <summary>
    /// Clase que representa una tasa de cambio para una cantidad específica en una moneda base.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public class Currency
    {
        /// <summary>
        /// Cantidad para la que se calculó la tasa de cambio.
        /// </summary>
        [SwaggerSchema(Description = "Cantidad para la que se calculó la tasa de cambio")]
        public required string Amount { get; set; } = "0"; // Valor predeterminado

        /// <summary>
        /// Moneda base utilizada en la conversión.
        /// </summary>
        [SwaggerSchema(Description = "Moneda base utilizada en la conversión")]
        public required string Base { get; set; } = "USD"; // Moneda base por defecto

        /// <summary>
        /// Fecha en la que se consultaron las tasas de cambio.
        /// </summary>
        [SwaggerSchema(Description = "Fecha en la que se consultaron las tasas de cambio")]
        public required string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd"); // Fecha actual por defecto

        /// <summary>
        /// Mapa de tasas de cambio por moneda.
        /// </summary>
        [JsonProperty("rates")]
        [SwaggerSchema(Description = "Mapa de tasas de cambio por moneda")]
        public required Dictionary<string, double> ExchangeRates { get; set; } = new(); // Diccionario vacío como valor predeterminado
    }
}
