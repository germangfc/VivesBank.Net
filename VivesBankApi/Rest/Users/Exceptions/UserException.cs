namespace VivesBankApi.Rest.Users.Exceptions;

/// <summary>
/// Excepción base para errores relacionados con el usuario.
/// </summary>
public class UserException(string message) : Exception(message);