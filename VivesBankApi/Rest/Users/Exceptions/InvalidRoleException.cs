namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidRoleException: UserException
{
    public InvalidRoleException(string role) : base($"The role {role} is not valid")
    {
    }
}