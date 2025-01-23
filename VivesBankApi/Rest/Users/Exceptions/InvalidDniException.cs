namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidDniException : UserException
{
    public InvalidDniException(string username) : base($"The dni {username} is not a valid DNI")
    {
    }
}