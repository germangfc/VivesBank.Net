namespace VivesBankApi.Rest.Users.Exceptions;

public class UserNotFoundException(String id) : UserException($"The user with id: {id} was not found");