/// <summary>
/// Excepción personalizada para errores relacionados con el almacenamiento de archivos.
/// </summary>
/// <remarks>
/// Esta excepción se utiliza para gestionar errores específicos que puedan ocurrir
/// durante el proceso de almacenamiento de archivos en el sistema, como problemas con
/// el servidor de almacenamiento, permisos, o errores al intentar guardar archivos.
/// </remarks>
public class FileStorageExceptions : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="FileStorageExceptions"/>.
    /// </summary>
    /// <param name="message">El mensaje que describe el error.</param>
    public FileStorageExceptions(string message) : base(message)
    {
    }
}