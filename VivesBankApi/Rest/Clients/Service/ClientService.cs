using System.Security.Claims;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Mappers;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Clients.storage;
using VivesBankApi.Rest.Clients.storage.Config;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Repository;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Clients.Service;

public class ClientService : IClientService
{
    private readonly ILogger _logger;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDatabase _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileStorageConfig _fileStorageConfig;
    private readonly FileStorageRemoteConfig _fileStorageRemoteConfig;
    
    public ClientService(
        ILogger<ClientService> logger,
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IConnectionMultiplexer connection,
        IHttpContextAccessor httpContextAccessor,
        FileStorageConfig fileStorageConfig,
        FileStorageRemoteConfig fileStorageRemoteConfig
        )
    {
        _userRepository = userRepository; 
        _logger = logger;
        _clientRepository = clientRepository;
        _cache = connection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _fileStorageConfig = fileStorageConfig;
        _fileStorageRemoteConfig = fileStorageRemoteConfig;
    } 
    public async Task<PagedList<ClientResponse>> GetAllClientsAsync(
        int pageNumber, 
        int pageSize,
        string fullName,
        bool? isDeleted,
        string direction)
    {
        var clients = await _clientRepository.GetAllClientsPagedAsync(pageNumber, pageSize, fullName, isDeleted, direction);
        var mappedClients = new PagedList<ClientResponse>(
            clients.Select(u => u.ToResponse()),
            clients.TotalCount,
            clients.PageNumber,
            clients.PageSize
        );
        return mappedClients;
    }
    
    public async Task<ClientResponse> GettingMyClientData()
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userRepository.GetByIdAsync(id);
        if (userForFound == null)
            throw new UserNotFoundException(id);
        var client = await _clientRepository.getByUserIdAsync(userForFound.Id);
        if (client == null)
            throw new ClientExceptions.ClientNotFoundException(userForFound.Id);
        return client.ToResponse();
    }

    public async Task<ClientResponse> GetClientByIdAsync(string id)
    {
        _logger.LogInformation($"Getting Client by id {id}");
        var res = await GetByIdAsync(id) ?? throw new ClientExceptions.ClientNotFoundException(id);
        return res.ToResponse();
    }

    public async Task<ClientResponse> CreateClientAsync(ClientRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userRepository.GetByIdAsync(id);
        if (userForFound == null)
            throw new UserNotFoundException(id);
        var existingClient = await _clientRepository.getByUserIdAsync(id);
        if (existingClient!= null)
            throw new ClientExceptions.ClientAlreadyExistsException(id);
        var client = request.FromDtoRequest();
        client.UserId = userForFound.Id;
        await _clientRepository.AddAsync(client);
        return client.ToResponse();
    }

    public async Task<ClientResponse> UpdateClientAsync(string id, ClientUpdateRequest request)
    {
        _logger.LogInformation($"Updating client with id {id}");
        var clientToUpdate = await GetByIdAsync(id) ??  throw new ClientExceptions.ClientNotFoundException(id);
        clientToUpdate.Adress = request.Address;
        clientToUpdate.FullName = request.FullName;
        clientToUpdate.UpdatedAt = DateTime.UtcNow;
        await _clientRepository.UpdateAsync(clientToUpdate);
        await _cache.KeyDeleteAsync(id); // Invalidating the cached client
        return clientToUpdate.ToResponse();
    }
    
    //TODO UPDATE PHOTO DNI  y Photo Perfil unicamente para el patch
    
    public async Task LogicDeleteClientAsync(string id)
    {
        _logger.LogInformation($"Setting Client with id {id} to deleted");
        var clientToDelete = await _clientRepository.GetByIdAsync(id)?? throw new ClientExceptions.ClientNotFoundException(id);
        clientToDelete.IsDeleted = true;
        await _clientRepository.UpdateAsync(clientToDelete);
    }
    
    private async Task<Client?> GetByIdAsync(string id)
    {
        // Try to get from cache first
        var cachedClient = await _cache.StringGetAsync(id);
        if (!cachedClient.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Client>(cachedClient);
        }

        Client? client = await _clientRepository.GetByIdAsync(id);
        if (client != null)
        {
            await _cache.StringSetAsync(id, JsonConvert.SerializeObject(client), TimeSpan.FromMinutes(10));
            return client;
        }
        return null;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string baseFileName)
    {
        _logger.LogInformation($"Saving file for user with base name: {baseFileName}");

        if (file.Length > _fileStorageConfig.MaxFileSize)
        {
            throw new FileStorageExceptions("El tamaño del fichero excede del máximo permitido");
        }

        var fileExtension = Path.GetExtension(file.FileName);
        if (!_fileStorageConfig.AllowedFileTypes.Contains(fileExtension))
        {
            throw new FileStorageExceptions("Tipo de fichero no permitido");
        }

        var uploadPath = Path.Combine(_fileStorageConfig.UploadDirectory);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var fullFileName = $"{baseFileName}-{timestamp}{fileExtension}";
        var filePath = Path.Combine(uploadPath, fullFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        _logger.LogInformation($"File saved: {fullFileName}");
        return fullFileName;
    }


    
    public async Task<string> UpdateClientPhotoAsync(string clientId, IFormFile file)
    {
        _logger.LogInformation($"Updating profile photo for client with ID: {clientId}");

        if (file == null || file.Length == 0)
        {
            throw new FileNotFoundException("No file was provided or the file is empty.");
        }

        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            throw new ClientExceptions.ClientNotFoundException($"Client with ID {clientId} not found.");
        }

        var user = await _userRepository.GetByIdAsync(client.UserId);
        if (user == null)
        {
            throw new UserNotFoundException(client.UserId);
        }

        var newFileName = await SaveFileAsync(file, $"PROFILE-{user.Dni}");

        if (client.Photo != "defaultId.png")
        {
            await DeleteFileAsync(client.Photo);
        }

        client.Photo = newFileName;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);

        _logger.LogInformation($"Profile photo updated successfully for client with ID: {clientId}");
        return newFileName;
    }


    
    public async Task<bool> DeleteFileAsync(string fileName)
    {
        _logger.LogInformation($"Deleting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                return false;
            }
            
            File.Delete(filePath);
            _logger.LogInformation($"File deleted: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            throw;
        }
    }

    

    public async Task<FileStream> GetFileAsync(string fileName)
    {
        _logger.LogInformation($"Getting file: {fileName}");
        try
        {
            var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);
            
            if (!File.Exists(filePath))
            {
                _logger.LogWarning($"File not found: {filePath}");
                throw new FileNotFoundException($"File not found: {fileName}");
            }
            
            _logger.LogInformation($"File found: {filePath}");
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file");
            throw;
        }
    }
    
    public Task<string> SaveFileToFtpAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<FileStream> GetFileFromFtpAsync(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFileFromFtpAsync(string fileName)
    {
        throw new NotImplementedException();
    }
}