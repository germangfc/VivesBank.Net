namespace VivesBankApi.Rest.Users.Exceptions;

public class InvalidUserException(String message) : UserException($"The DNI {message} is not valid");