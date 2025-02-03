namespace VivesBankApi.Utils.GenericStorage.JSON;

/// <summary>
/// Interfaz que define los métodos para importar y exportar entidades de tipo <typeparamref name="T"/> a y desde archivos JSON.
/// </summary>
/// <typeparam name="T">Tipo de la entidad que se maneja en la interfaz.</typeparam>
public interface IGenericStorageJson<T> where T : class
{
    /// <summary>
    /// Importa entidades de tipo <typeparamref name="T"/> desde un archivo JSON proporcionado.
    /// </summary>
    /// <param name="fileStream">El archivo que contiene los datos en formato JSON.</param>
    /// <returns>Un observable de entidades <typeparamref name="T"/> que se emiten conforme se leen desde el archivo.</returns>
    /// <remarks>
    /// Este método permite importar las entidades de forma asíncrona y manejarlas cuando se leen del archivo.
    /// </remarks>
    IObservable<T> Import(IFormFile fileStream);

    /// <summary>
    /// Exporta una lista de entidades de tipo <typeparamref name="T"/> a un archivo JSON.
    /// </summary>
    /// <param name="entities">La lista de entidades a exportar.</param>
    /// <returns>Un <see cref="FileStream"/> que apunta al archivo JSON generado.</returns>
    /// <remarks>
    /// El archivo JSON generado se guardará en un directorio específico y el nombre incluirá la fecha y hora actuales.
    /// </remarks>
    Task<FileStream> Export(List<T> entities);

    /// <summary>
    /// Importa una lista de entidades de tipo <typeparamref name="T"/> desde un archivo JSON ubicado en la ruta especificada.
    /// </summary>
    /// <param name="filePath">La ruta del archivo JSON que contiene las entidades.</param>
    /// <returns>Una lista de entidades de tipo <typeparamref name="T"/> deserializadas desde el archivo.</returns>
    /// <remarks>
    /// Si el archivo no existe o no contiene datos válidos, se devuelve una lista vacía.
    /// </remarks>
    Task<List<T>> ImportFromFile(string filePath);
}
