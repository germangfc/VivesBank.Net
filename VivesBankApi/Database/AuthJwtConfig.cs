namespace VivesBankApi.Database;

public class AuthJwtConfig
{
    public String Key { get; set; } = String.Empty;
    public String Issuer { get; set; } = String.Empty;
    public String Audience { get; set; } = String.Empty;
    public String ExpiresInMinutes { get; set; } = String.Empty;
}