namespace VivesBankApi.Rest.Product.BankAccounts.AccountTypeExtensions;

/// <summary>
/// Proporciona métodos de extensión para la enumeración <see cref="AccountType"/>.
/// </summary>
/// <remarks>
/// Esta clase contiene métodos de extensión que permiten obtener información adicional sobre el tipo de cuenta, como la tasa de interés asociada.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public static class AccountTypeExtensions
{
    /// <summary>
    /// Diccionario que almacena las tasas de interés asociadas a cada tipo de cuenta.
    /// </summary>
    private static readonly Dictionary<AccountType, double> InterestRates = new Dictionary<AccountType, double>
    {
        { AccountType.SAVING, 0.2 },   // Tasa de interés para cuentas de ahorro
        { AccountType.STANDARD, 0.0 }   // No hay tasa de interés para cuentas estándar
    };

    /// <summary>
    /// Obtiene la tasa de interés asociada a un tipo de cuenta.
    /// </summary>
    /// <param name="accountType">El tipo de cuenta para el que se desea obtener la tasa de interés.</param>
    /// <returns>La tasa de interés asociada al tipo de cuenta.</returns>
    /// <remarks>
    /// Si el tipo de cuenta es <see cref="AccountType.SAVING"/>, la tasa será del 0.2. 
    /// Si es <see cref="AccountType.STANDARD"/>, no tendrá tasa de interés (0.0).
    /// </remarks>
    public static double GetInterestRate(this AccountType accountType)
    {
        return InterestRates[accountType];
    }
}


