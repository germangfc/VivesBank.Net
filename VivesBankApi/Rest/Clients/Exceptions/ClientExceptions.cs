namespace VivesBankApi.Rest.Clients.Exceptions;

public abstract class ClientExceptions : Exception
{
    protected ClientExceptions(String message) : base(message)
    {
    }

    public class ClientNotFoundException(String id) : ClientExceptions($"Client not found by id {id}")
    {
    }

    public class ClientAlreadyExistsException(string id)
        : ClientExceptions($"A client already exists with this user id {id}")
    {
    }

    public class ClientNotAllowedToAccessAccount(String id, String Iban)
        : ClientExceptions($"The client with id {id} is not allowed to access the account with iban {Iban}")
    {
    }
}    