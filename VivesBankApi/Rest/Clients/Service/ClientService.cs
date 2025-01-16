using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Mappers;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Repository;

namespace VivesBankApi.Rest.Clients.Service;

public class ClientService : IClientService
{
    private readonly ILogger _logger;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;

    public ClientService(ILogger<ClientService> logger, IUserRepository userRepository, IClientRepository clientRepository)
    {
        _userRepository = userRepository; 
        _logger = logger;
        _clientRepository = clientRepository;
    } 
    public async Task<List<ClientResponse>> GetAllAsync()
    {
        _logger.LogInformation("Getting all clients");
        var result = await _clientRepository.GetAllAsync();
        return result.Select(c => c.toResponse()).ToList();
    }

    public async Task<ClientResponse> GetClientByIdAsync(string id)
    {
        _logger.LogInformation($"Getting Client by id {id}");
        var res = await _clientRepository.GetByIdAsync(id)??throw new ClientExceptions.ClientNotFoundException(id);
        return res.toResponse();
    }

    public async Task<ClientResponse> CreateClientAsync(ClientRequest request)
    {
        _logger.LogInformation("Creating client");
        if (await _userRepository.GetByIdAsync(request.UserId) == null)
            throw new UserNotFoundException(request.UserId);
        var newClient = request.fromDtoRequest();
        await _clientRepository.AddAsync(newClient);
        return newClient.toResponse();
    }

    public async Task<ClientResponse> UpdateClientAsync(string id, ClientUpdateRequest request)
    {
        _logger.LogInformation($"Updating client with id {id}");
        var clientToUpdate = await _clientRepository.GetByIdAsync(id)??  throw new ClientExceptions.ClientNotFoundException(id);
        clientToUpdate.Adress = request.Address;
        clientToUpdate.FullName = request.FullName;
        clientToUpdate.UpdatedAt = DateTime.Now;
        await _clientRepository.UpdateAsync(clientToUpdate);
        return clientToUpdate.toResponse();
    }
    
    //TODO UPDATE PHOTO DNI  y Photo Perfil unicamente para el patch

    public Task DeleteClientAsync(string id)
    {
        _logger.LogInformation($"Deleting Client with id {id}");
        return _clientRepository.DeleteAsync(id);
    }

    public async Task LogicDeleteClientAsync(string id)
    {
        _logger.LogInformation($"Setting Client with id {id} to deleted");
        var clientToDelete = await _clientRepository.GetByIdAsync(id)?? throw new ClientExceptions.ClientNotFoundException(id);
        clientToDelete.IsDeleted = true;
        await _clientRepository.UpdateAsync(clientToDelete);
    }
}