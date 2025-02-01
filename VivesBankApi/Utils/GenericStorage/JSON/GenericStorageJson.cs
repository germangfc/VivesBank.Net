using System.Reactive.Linq;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace VivesBankApi.Utils.GenericStorage.JSON;

public class GenericStorageJson<T> : IGenericStorageJson<T> where T : class
{
    protected readonly ILogger<GenericStorageJson<T>> _logger;
    
    public GenericStorageJson(ILogger<GenericStorageJson<T>> logger)
    {
        _logger = logger;
    }
    
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
}