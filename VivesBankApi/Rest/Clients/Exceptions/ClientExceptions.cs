namespace VivesBankApi.Rest.Clients.Exceptions;

public abstract class ClientExceptions : Exception
{
    protected ClientExceptions(String message) : base(message)
    {
    }
    public class ClientNotFoundException(String id) : ClientExceptions($"Client not found by id {id}")
    {
    }
    
    
}