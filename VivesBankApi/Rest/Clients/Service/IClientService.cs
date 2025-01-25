using VivesBankApi.Rest.Clients.Dto;

namespace VivesBankApi.Rest.Clients.Service;

public interface IClientService
{
    public Task<PagedList<ClientResponse>> GetAllClientsAsync(
        int pageNumber,
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction);
    Task<ClientResponse> GetClientByIdAsync(string id);
    Task<ClientResponse> GettingMyClientData();
    Task<ClientResponse> CreateClientAsync(ClientRequest request);
    Task<ClientResponse> UpdateClientAsync(string id, ClientUpdateRequest request);
    Task LogicDeleteClientAsync(string id);
    
    //Funciones para storage en Local

    Task<String> SaveFileAsync(IFormFile file, string baseFileName);
    
    Task<string> UpdateClientPhotoAsync(string clientId, IFormFile file);
    Task<FileStream> GetFileAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
    
    //Funciones para storage remoto FTP
    Task<string> SaveFileToFtpAsync(IFormFile file);
    Task<FileStream> GetFileFromFtpAsync(string fileName);
    Task DeleteFileFromFtpAsync(string fileName);
}