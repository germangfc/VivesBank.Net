namespace VivesBankApi.Rest.Products.BankAccounts.Exceptions;

public class AccountsExceptions(String message) : Exception(message)
{
    public class AccountNotFoundException(String id) : AccountsExceptions($"Account not found by id {id}");

    public class AccountNotCreatedException()
        : AccountsExceptions($"Account couldnt be created, check that te client and the product exists");

    public class AccountIbanNotGeneratedException() : AccountsExceptions($"Iban Couldnt be created after 1000 tries");
}