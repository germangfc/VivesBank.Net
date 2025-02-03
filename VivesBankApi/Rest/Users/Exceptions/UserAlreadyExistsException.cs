namespace VivesBankApi.Rest.Users.Exceptions;

/// <summary>
/// Excepción que se lanza cuando ya existe un usuario con el nombre de usuario proporcionado.
/// </summary>
public class UserAlreadyExistsException(string username)
    : UserException($"A user with the username '{username}' already exists.");