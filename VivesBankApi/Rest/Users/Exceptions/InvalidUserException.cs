namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidUserException : UserException
{
    public InvalidUserException(String role) : base($"The role {role} is not valid")
    {
    }
}