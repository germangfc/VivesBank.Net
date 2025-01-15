namespace VivesBankApi.Rest.Users.Exceptions;

public class UserNotFoundException : UserException
{
    public UserNotFoundException(String id) : base($"The user with id: {id} was not found")
    {
    }
}