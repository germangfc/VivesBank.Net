namespace VivesBankApi.Utils.IbanGenerator;

/// <summary>
/// Interfaz que define el contrato para un generador de IBANs (International Bank Account Number).
/// </summary>
public interface IIbanGenerator
{
    /// <summary>
    /// Genera un IBAN único de manera asincrónica.
    /// </summary>
    /// <returns>
    /// Un IBAN único generado como una cadena.
    /// </returns>
    /// <remarks>
    /// Este método debe ser implementado por una clase que proporcione la lógica necesaria
    /// para generar un IBAN válido, único y conforme a las especificaciones internacionales.
    /// </remarks>
    Task<string> GenerateUniqueIbanAsync();
}
