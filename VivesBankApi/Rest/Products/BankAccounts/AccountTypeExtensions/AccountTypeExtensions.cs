namespace VivesBankApi.Products.BankAccounts.AccountTypeExtensions;
using System.Collections.Generic;

public static class AccountTypeExtensions
{
    private static readonly Dictionary<AccountType, double> InterestRates = new Dictionary<AccountType, double>
    {
        { AccountType.SAVING, 0.2 },   
        { AccountType.STANDARD, 0.0 }
    };
    public static double GetInterestRate(this AccountType accountType)
    {
        return InterestRates[accountType];
    }
}

