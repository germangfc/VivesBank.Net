﻿using VivesBankApi.Rest.Clients.Dto;

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
    
    Task<String> SaveFileAsync(IFormFile file);

    Task<string> UpdateClientDniPhotoAsync(string clientId, IFormFile file);

    Task<string> UpdateClientPhotoAsync(string clientId, IFormFile file);
    
    Task<FileStream> GetFileAsync(string fileName);
    
    Task<bool> DeleteFileAsync(string fileName);
}