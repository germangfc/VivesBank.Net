using Newtonsoft.Json;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Utils.GenericStorage.JSON;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Clients.storage.JSON;

public class ClientStorageJson(ILogger<ClientStorageJson> logger) : GenericStorageJson<Client>(logger), IClientStorageJson
{
    public async Task<FileStream> ExportOnlyMeData(Client client)
    {
        _logger.LogInformation($"Exporting Client to a JSON file");
        var json = JsonConvert.SerializeObject(client, Formatting.Indented);
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = "Client_WithId_" + client.Id + "_" + "InSystem-" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json";
        var filePath = Path.Combine(directoryPath, fileName);

        await File.WriteAllTextAsync(filePath, json);

        _logger.LogInformation($"File written to: {filePath}");

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}