namespace VivesBankApi.Rest.Users.Exceptions;

public class UserAlreadyExistsException(string username)
    : UserException($"A user with the username '{username}' already exists.");