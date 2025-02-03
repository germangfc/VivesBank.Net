namespace VivesBankApi.Utils.ApiConfig;

/// <summary>
/// Configuración relacionada con la API, como la URL base del servicio.
/// </summary>
public class ApiConfig
{
    /// <summary>
    /// La URL base del endpoint de la API.
    /// </summary>
    /// <remarks>
    /// Este valor es utilizado para construir las rutas completas de las solicitudes a la API.
    /// Ejemplo: "https://api.miservicio.com".
    /// </remarks>
    public string BaseEndpoint { get; set; }
}
