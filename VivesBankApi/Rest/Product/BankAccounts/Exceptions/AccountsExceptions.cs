namespace VivesBankApi.Rest.Products.BankAccounts.Exceptions;

public class AccountsExceptions(String message) : Exception(message)
{
    public class AccountNotFoundException(String id) : AccountsExceptions($"Account not found by id {id}");

    public class AccountNotFoundByIban(String Iban) : AccountsExceptions($"Account not found by IBAN {Iban}");

    public class AccountNotCreatedException()
        : AccountsExceptions($"Account couldnt be created, check that te client and the product exists");

    public class AccountIbanNotGeneratedException() : AccountsExceptions($"Iban Couldnt be created after 1000 tries");
    
    public class AccountUnknownIban(String Iban) : AccountsExceptions($"Unknown Iban {Iban}");

    public class AccountNotUpdatedException(string id)
        : AccountsExceptions($"Account couldn't be updated, Id = {id}, check that te client and the product exists");

    public class AccountIbanNotValid(string iban) : AccountsExceptions($"Account IBAN number is not valid: {iban}");
}