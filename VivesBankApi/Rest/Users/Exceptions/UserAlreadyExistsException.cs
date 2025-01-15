namespace VivesBankApi.Rest.Users.Exceptions;

public class UserAlreadyExistsException : UserException
{
    public UserAlreadyExistsException(string username)
        : base($"A user with the username '{username}' already exists.")
    {
    }
}