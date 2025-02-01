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
    Task<ClientResponse> GetClientByUserIdAsync(string userId);
    Task<ClientResponse> GettingMyClientData();
    Task<String> CreateClientAsync(ClientRequest request);
    Task<ClientResponse> UpdateClientAsync(string id, ClientUpdateRequest request);
    Task<ClientResponse> UpdateMeAsync(ClientUpdateRequest request);
    Task LogicDeleteClientAsync(string id);
    
   Task DeleteMe();

    //Funciones para storage en Local

    Task<String> SaveFileAsync(IFormFile file, string baseFileName);
    Task<string> UpdateClientPhotoAsync(string clientId, IFormFile file);
    
    Task<FileStream> GetFileAsync(string fileName);

    Task<FileStream> GettingMyProfilePhotoAsync();
    Task<string> UpdateMyProfilePhotoAsync(IFormFile file);

    //Funciones para storage remoto FTP
    Task<string> SaveFileToFtpAsync(IFormFile file, string dni);
    Task<FileStream> GetFileFromFtpAsync(string fileName);
    Task<string> UpdateClientPhotoDniAsync(string userId, IFormFile file);
    Task<string> UpdateMyPhotoDniAsync(IFormFile file);
    Task<FileStream> GettingMyDniPhotoFromFtpAsync();
}