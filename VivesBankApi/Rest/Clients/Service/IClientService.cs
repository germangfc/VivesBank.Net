using VivesBankApi.Rest.Clients.Dto;

namespace VivesBankApi.Rest.Clients.Service;

public interface IClientService
{
    Task<List<ClientResponse>> GetAllAsync();
    Task<ClientResponse> GetClientByIdAsync(string id);
    Task<ClientResponse> CreateClientAsync(ClientRequest request);
    Task<ClientResponse> UpdateClientAsync(string id, ClientUpdateRequest request);
    Task LogicDeleteClientAsync(string id);
}