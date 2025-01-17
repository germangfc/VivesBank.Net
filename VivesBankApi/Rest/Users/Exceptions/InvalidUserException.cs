namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidUserException : UserException
{
    public InvalidUserException(String message) : base(message)
    {
    }
}