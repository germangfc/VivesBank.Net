using ApiFranfurkt.Properties.Currency.Services;
using Refit;

namespace ApiFrankfurt.Configuration
{
    /// <summary>
    /// Configuración para añadir los servicios de Frankfurter a la colección de servicios.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public static class CurrencyRefitConfig
    {
        /// <summary>
        /// Agrega los servicios de Frankfurter a la colección de servicios de la aplicación.
        /// Configura la URL base y el tiempo de espera para las solicitudes.
        /// </summary>
        /// <param name="services">Colección de servicios de la aplicación.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public static void AddFrankFurterServices(this IServiceCollection services, IConfiguration configuration)
        {
            string baseUrl = configuration["Frankfurter:BaseUrl"] ?? "https://api.frankfurter.app";
            
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("Frankfurter BaseUrl is not configured properly.");
            }
            
            Console.WriteLine($"Base URL from configuration: {baseUrl}");
            
            services.AddRefitClient<ICurrencyApiService>()
                .ConfigureHttpClient(client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
        }
    }

    /// <summary>
    /// Representa la respuesta con las tasas de cambio de la API de Frankfurter.
    /// </summary>
    public class ExchangeRateResponse
    {
        /// <summary>
        /// Moneda base de las tasas de cambio.
        /// </summary>
        public string Base { get; set; } = string.Empty;

        /// <summary>
        /// Fecha en la que se consultaron las tasas de cambio.
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// Diccionario de tasas de cambio por moneda.
        /// </summary>
        public Dictionary<string, double> Rates { get; set; } = new Dictionary<string, double>();
    }
}
