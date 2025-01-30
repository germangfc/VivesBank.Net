namespace VivesBankApi.Rest.Products.BankAccounts.Exceptions;

public class AccountsExceptions(String message) : Exception(message)
{
    public class AccountNotFoundException(String id) : AccountsExceptions($"Account not found by id {id}");

    public class AccountNotFoundByIban(String Iban) : AccountsExceptions($"Account not found by IBAN {Iban}");

    public class AccountNotCreatedException()
        : AccountsExceptions($"Account couldnt be created, check that te client and the product exists");

    public class AccountIbanNotGeneratedException() : AccountsExceptions($"Iban Couldnt be created after 1000 tries");

    public class AccountNotDeletedException(String iban)
        : AccountsExceptions($"Account could not be deleted because you don´t own it.");

    public class AccountWithBalanceException(String iban)
        : AccountsExceptions($"Account with {iban} cannot be deleted because it has money on it.");
    
    public class AccountUnknownIban(String Iban) : AccountsExceptions($"Unknown Iban {Iban}");

    public class AccountNotUpdatedException(string id)
        : AccountsExceptions($"Account couldn't be updated, Id = {id}, check that te client and the product exists");

    public class AccountIbanNotValid(string iban) : AccountsExceptions($"Account IBAN number is not valid: {iban}");
}