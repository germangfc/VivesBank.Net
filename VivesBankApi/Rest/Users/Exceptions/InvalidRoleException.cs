namespace VivesBankApi.Rest.Users.Exceptions;

/// <summary>
/// Excepción que se lanza cuando el rol proporcionado no es válido.
/// </summary>
public class InvalidRoleException : UserException
{
    /// <summary>
    /// Constructor de la excepción. Inicializa un mensaje de error que indica que el rol no es válido.
    /// </summary>
    /// <param name="role">El rol que es inválido.</param>
    public InvalidRoleException(string role) 
        : base($"The role {role} is not valid")
    {
    }
}
