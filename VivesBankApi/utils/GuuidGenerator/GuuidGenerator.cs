using System.Security.Cryptography;

namespace VivesBankApi.utils.GuuidGenerator;

public class GuuidGenerator
{
    public static string GenerateHash()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] bytes = new byte[8];
            rng.GetBytes(bytes);
            string base64 = Convert.ToBase64String(bytes)
                .TrimEnd('=') 
                .Replace('+', '-') 
                .Replace('/', '_');
            return base64;
        }
    }
}