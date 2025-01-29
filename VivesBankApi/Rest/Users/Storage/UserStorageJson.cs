using System.Reactive.Linq;
using Newtonsoft.Json;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Users.Storage;

public class UserStorageJson : IUserStorageJson
{
    private ILogger<UserStorageJson> _logger;
    
    public UserStorageJson(ILogger<UserStorageJson> logger)
    {
        _logger = logger;
    }
    
    public IObservable<User> Import(IFormFile fileStream)
    {
        _logger.LogInformation("Importing Users from a JSON file");

        return Observable.Create<User>(async (observer, cancellationToken) =>
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
                        var obj = serializer.Deserialize<User>(jsonReader);
                        observer.OnNext(obj);
                    }
                }
                observer.OnCompleted();
            }catch (Exception ex)
            {
                observer.OnError(ex);
            }
        });
    }


    public async Task<FileStream> Export(List<User> users)
    {
        _logger.LogInformation("Exporting users to a Json file");
        var json = JsonConvert.SerializeObject(users, Formatting.Indented);
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        await File.WriteAllTextAsync(tempFilePath, json);
        return new FileStream(tempFilePath, FileMode.Open);
    }
}