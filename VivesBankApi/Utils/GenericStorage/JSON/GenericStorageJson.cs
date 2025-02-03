using System.Reactive.Linq;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace VivesBankApi.Utils.GenericStorage.JSON;

/// <summary>
/// Clase genérica para importar y exportar entidades de tipo <typeparamref name="T"/> desde y hacia archivos JSON.
/// </summary>
/// <typeparam name="T">Tipo de la entidad que se maneja en la clase.</typeparam>
public class GenericStorageJson<T> : IGenericStorageJson<T> where T : class
{
    protected readonly ILogger<GenericStorageJson<T>> _logger;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GenericStorageJson{T}"/>.
    /// </summary>
    /// <param name="logger">El logger utilizado para registrar eventos.</param>
    public GenericStorageJson(ILogger<GenericStorageJson<T>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Importa entidades de tipo <typeparamref name="T"/> desde un archivo JSON.
    /// </summary>
    /// <param name="fileStream">El archivo que contiene los datos en formato JSON.</param>
    /// <returns>Un observable de entidades <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Lee el archivo JSON y emite cada entidad del tipo <typeparamref name="T"/> presente en el archivo.
    /// </remarks>
    public IObservable<T> Import(IFormFile fileStream)
    {
        _logger.LogInformation($"Importing {typeof(T).Name} from a JSON file");
        return Observable.Create<T>(async (observer, cancellationToken) =>
        {
            try
            {
                using var stream = fileStream.OpenReadStream();
                using var streamReader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(streamReader)
                {
                    SupportMultipleContent = true
                };

                var serializer = new JsonSerializer
                {
                    MissingMemberHandling = MissingMemberHandling.Error
                };

                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var obj = serializer.Deserialize<T>(jsonReader);
                        observer.OnNext(obj);
                    }
                }
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        });
    }

    /// <summary>
    /// Exporta una lista de entidades de tipo <typeparamref name="T"/> a un archivo JSON.
    /// </summary>
    /// <param name="entities">La lista de entidades a exportar.</param>
    /// <returns>Un <see cref="FileStream"/> que apunta al archivo JSON generado.</returns>
    /// <remarks>
    /// El archivo JSON se guarda en el directorio "uploads/Json" y el nombre del archivo incluye la fecha y hora actual.
    /// </remarks>
    public async Task<FileStream> Export(List<T> entities)
    {
        _logger.LogInformation($"Exporting {typeof(T).Name} to a JSON file");

        var json = JsonConvert.SerializeObject(entities, Formatting.Indented);
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = $"{typeof(T).Name}sInSystem-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        var filePath = Path.Combine(directoryPath, fileName);

        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation($"File written to: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    /// <summary>
    /// Importa una lista de entidades de tipo <typeparamref name="T"/> desde un archivo JSON ubicado en la ruta especificada.
    /// </summary>
    /// <param name="filePath">La ruta del archivo JSON que contiene las entidades.</param>
    /// <returns>Una lista de entidades de tipo <typeparamref name="T"/>.</returns>
    /// <remarks>
    /// Si el archivo no existe o no contiene datos válidos, se devuelve una lista vacía.
    /// </remarks>
    public async Task<List<T>> ImportFromFile(string filePath)
    {
        _logger.LogInformation($"Importing {typeof(T).Name} from file: {filePath}");

        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"File not found: {filePath}");
            return new List<T>();
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
    }
}
