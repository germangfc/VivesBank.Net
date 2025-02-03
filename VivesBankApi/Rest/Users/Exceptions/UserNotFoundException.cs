namespace VivesBankApi.Rest.Users.Exceptions;

/// <summary>
/// Excepción base para errores relacionados con el usuario.
/// </summary>
public class UserNotFoundException(String id) : UserException($"The user with id: {id} was not found");