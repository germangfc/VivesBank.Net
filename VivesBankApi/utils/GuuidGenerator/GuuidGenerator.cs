using System.Security.Cryptography;

namespace VivesBankApi.utils.GuuidGenerator;

/// <summary>
/// Generador de identificadores únicos basado en un hash utilizando un generador de números aleatorios criptográficamente seguro.
/// </summary>
public class GuuidGenerator
{
    /// <summary>
    /// Genera un identificador único utilizando un valor hash codificado en Base64.
    /// </summary>
    /// <returns>
    /// Un identificador único generado como una cadena en formato Base64 URL-safe (sin signos de igual, con los caracteres '+' y '/' reemplazados).
    /// </returns>
    /// <remarks>
    /// Este método utiliza <see cref="RNGCryptoServiceProvider"/> para generar un valor aleatorio de 8 bytes, 
    /// luego lo codifica en Base64 y lo ajusta para hacerlo compatible con URLs (sin los caracteres '+' y '/').
    /// El valor final se devuelve como una cadena de caracteres sin los signos de igual al final.
    /// </remarks>
    public static string GenerateHash()
    {
        // Usamos RNGCryptoServiceProvider para generar números aleatorios seguros.
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] bytes = new byte[8]; // Generamos 8 bytes aleatorios.
            rng.GetBytes(bytes); // Llenamos el arreglo con valores aleatorios.

            // Convertimos los bytes en una cadena en Base64, ajustando los caracteres para la compatibilidad con URLs.
            string base64 = Convert.ToBase64String(bytes)
                .TrimEnd('=') // Eliminamos los signos de igual al final (requerido para la URL-safe).
                .Replace('+', '-') // Reemplazamos '+' por '-' para hacerlo compatible con URL.
                .Replace('/', '_'); // Reemplazamos '/' por '_' para hacerlo compatible con URL.

            return base64; // Devolvemos el valor generado como cadena.
        }
    }
}
