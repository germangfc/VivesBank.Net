namespace VivesBankApi.Rest.Clients.Storage.Service
{
    /// <summary>
    /// Interfaz para servicios que gestionan el almacenamiento de archivos.
    /// </summary>
    /// <remarks>
    /// Esta interfaz define las operaciones básicas para guardar archivos. Implementaciones concretas
    /// deben proporcionar detalles sobre cómo se almacenarán los archivos, como el tipo de almacenamiento
    /// (por ejemplo, almacenamiento local, almacenamiento en la nube, etc.).
    /// </remarks>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo en el sistema de almacenamiento configurado.
        /// </summary>
        /// <param name="file">El archivo que se desea guardar.</param>
        /// <param name="baseFileName">El nombre base que se usará para el archivo. Este nombre puede ser modificado con una marca de tiempo u otros elementos.</param>
        /// <returns>Una tarea que representa la operación asincrónica. El valor de retorno es la ruta del archivo guardado.</returns>
        /// <remarks>
        /// Este método puede lanzar excepciones si el archivo no es válido (por ejemplo, si excede el tamaño máximo permitido
        /// o si el tipo de archivo no es permitido).
        /// </remarks>
        Task<string> SaveFileAsync(IFormFile file, string baseFileName);
    }
}