namespace VivesBankApi.Rest.Users.Exceptions;

/// <summary>
/// Excepción que se lanza cuando el DNI proporcionado no es válido.
/// </summary>
public class InvalidDniException : UserException
{
    /// <summary>
    /// Constructor de la excepción. Inicializa un mensaje de error que indica que el DNI no es válido.
    /// </summary>
    /// <param name="username">El DNI que es inválido.</param>
    public InvalidDniException(string username) 
        : base($"The dni {username} is not a valid DNI")
    {
    }
}
