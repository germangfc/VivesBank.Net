using Newtonsoft.Json;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Utils.GenericStorage.JSON;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Clients.storage.JSON;

public class ClientStorageJson : GenericStorageJson<ClientResponse>, IClientStorageJson
{
    public ClientStorageJson(ILogger<GenericStorageJson<ClientResponse>> logger) : base(logger)
    {
    }

    public async Task<FileStream> ExportOnlyMeData(ClientResponse user)
    {
        _logger.LogInformation($"Exporting Client to a JSON file");
        var json = JsonConvert.SerializeObject(user, Formatting.Indented);
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        await File.WriteAllTextAsync(tempFilePath, json);
        return new FileStream(tempFilePath, FileMode.Open);
    }
}