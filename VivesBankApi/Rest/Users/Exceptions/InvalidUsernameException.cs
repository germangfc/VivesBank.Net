namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidUsernameException : UserException
{
    public InvalidUsernameException(string username) : base($"The username {username} is not a valid DNI")
    {
    }
}