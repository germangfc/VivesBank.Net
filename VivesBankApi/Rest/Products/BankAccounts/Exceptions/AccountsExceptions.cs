namespace VivesBankApi.Rest.Products.BankAccounts.Exceptions;

public class AccountsExceptions(String message) : Exception(message)
{
    public class AccountNotFoundException(String id) : AccountsExceptions($"Account not found by id {id}");
}