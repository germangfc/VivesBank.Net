namespace VivesBankApi.Rest.Clients.Storage.Service;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string baseFileName);
}
