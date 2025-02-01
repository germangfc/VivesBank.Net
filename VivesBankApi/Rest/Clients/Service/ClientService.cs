using System.Security.Claims;
using FluentFTP;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Middleware.Jwt;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Mappers;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Clients.storage;
using VivesBankApi.Rest.Clients.storage.Config;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;
using Path = System.IO.Path;
using Role = VivesBankApi.Rest.Users.Models.Role;

namespace VivesBankApi.Rest.Clients.Service;

public class ClientService : IClientService
{
    private readonly ILogger _logger;
    private readonly IClientRepository _clientRepository;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly IDatabase _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileStorageConfig _fileStorageConfig;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IWebsocketHandler _websocketHandler;
    private readonly FileStorageRemoteConfig _fileStorageRemoteConfig;
    private readonly IConfiguration _configuration;
    
    public ClientService(
        ILogger<ClientService> logger,
        IUserService userService,
        IClientRepository clientRepository,
        IConnectionMultiplexer connection,
        IHttpContextAccessor httpContextAccessor,
        FileStorageConfig fileStorageConfig,
        IWebsocketHandler websocketHandler,
        IJwtGenerator jwtGenerator,
        IConfiguration configuration
        )
    {
        _jwtGenerator = jwtGenerator;
        _userService = userService; 
        _logger = logger;
        _clientRepository = clientRepository;
        _cache = connection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _fileStorageConfig = fileStorageConfig;
        _websocketHandler = websocketHandler;
        _fileStorageRemoteConfig = configuration.GetSection("FileStorageRemoteConfig").Get<FileStorageRemoteConfig>();
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
        var userForFound = await _userService.GetUserByIdAsync(id);
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

    public async Task<string> CreateClientAsync(ClientRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
    
        if (userForFound == null)
            throw new UserNotFoundException(id);
    
        var existingClient = await _clientRepository.getByUserIdAsync(id);
        if (existingClient != null)
            throw new ClientExceptions.ClientAlreadyExistsException(id);
    
        var userUpdate = new UserUpdateRequest
        {
            Role = Role.Client.ToString(),
        };
    
        var client = request.FromDtoRequest();
        client.UserId = id;

        client.Photo = "defaultProfile.png";
        client.PhotoDni = "defaultDni.png";
    
        await _userService.UpdateUserAsync(id, userUpdate);
    
        await _clientRepository.AddAsync(client);

        var updatedUser = await _userService.GetUserByIdAsync(id);
        _logger.LogDebug($"Updating user for client role: {updatedUser.Role}");

        return _jwtGenerator.GenerateJwtToken(updatedUser.ToUser());
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

    public async Task<ClientResponse> UpdateMeAsync(ClientUpdateRequest request)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
        _logger.LogInformation($"Updating client with id {id}");
        var clientToUpdate = await _clientRepository.getByUserIdAsync(id) ??  throw new ClientExceptions.ClientNotFoundException(id);
        clientToUpdate.Adress = request.Address;
        clientToUpdate.FullName = request.FullName;
        clientToUpdate.UpdatedAt = DateTime.UtcNow;
        await _clientRepository.UpdateAsync(clientToUpdate);
        await _cache.KeyDeleteAsync(id);
        await EnviarNotificacionUpdateAsync(clientToUpdate.ToResponse());
        return clientToUpdate.ToResponse();
    }
    
    
    public async Task LogicDeleteClientAsync(string id)
    {
        _logger.LogInformation($"Setting Client with id {id} to deleted");
        var clientToDelete = await _clientRepository.GetByIdAsync(id)?? throw new ClientExceptions.ClientNotFoundException(id);
        clientToDelete.IsDeleted = true;
        var userToDelete = await _userService.GetUserByIdAsync(clientToDelete.UserId);
        var userUpdate = new UserUpdateRequest
        {
            Role = Role.Revoked.ToString(),
            IsDeleted = false
        };
        await _userService.UpdateUserAsync(clientToDelete.UserId, userUpdate);
        await _clientRepository.UpdateAsync(clientToDelete);
    }

    public async Task DeleteMe()
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
    
        if (userForFound == null)
            throw new UserNotFoundException(id);
    
        var existingClient = await _clientRepository.getByUserIdAsync(id);
        if (existingClient == null)
            throw new ClientExceptions.ClientNotFoundException(id);
        
        existingClient.IsDeleted = true;
        await _clientRepository.UpdateAsync(existingClient);
        await _userService.DeleteUserAsync(id, logically: true);
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

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
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

        var user = await _userService.GetUserByIdAsync(client.UserId);
        if (user == null)
        {
            throw new UserNotFoundException(client.UserId);
        }

        var newFileName = await SaveFileAsync(file, $"PROFILE-{user.Dni}");


        client.Photo = newFileName;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);

        _logger.LogInformation($"Profile photo updated successfully for client with ID: {clientId}");
        return newFileName;
    }
    
    public async Task<string> UpdateMyProfilePhotoAsync(IFormFile file)
    {
        _logger.LogInformation("Updating profile photo for current user.");

        var user = _httpContextAccessor.HttpContext!.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var client = await _clientRepository.getByUserIdAsync(userId);
        if (client == null)
        {
            throw new ClientExceptions.ClientNotFoundException($"Client with user ID {userId} not found.");
        }

        var userData = await _userService.GetUserByIdAsync(client.UserId);
        if (userData == null)
        {
            throw new UserNotFoundException(client.UserId);
        }

        var newFileName = await SaveFileAsync(file, $"PROFILE-{userData.Dni}");
        

        client.Photo = newFileName;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);

        _logger.LogInformation($"Profile photo updated successfully for user ID: {userId} (DNI: {userData.Dni})");
        return newFileName;
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
    
   public async Task<string> SaveFileToFtpAsync(IFormFile file, string fileName)
{
    try
    {
        if (_fileStorageRemoteConfig == null ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpHost) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpUsername) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpPassword) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpDirectory))
        {
            throw new InvalidOperationException("Configuración de almacenamiento remoto inválida o incompleta.");
        }

        if (!_fileStorageRemoteConfig.AllowedFileTypes.Contains(Path.GetExtension(file.FileName).ToLower()) ||
            file.Length > _fileStorageRemoteConfig.MaxFileSize)
        {
            throw new InvalidOperationException("Archivo no permitido por tipo o tamaño.");
        }

        using (var client = new AsyncFtpClient(
                _fileStorageRemoteConfig.FtpHost,
                _fileStorageRemoteConfig.FtpUsername,
                _fileStorageRemoteConfig.FtpPassword))
        {
            client.Config.ConnectTimeout = 30000;
            client.Config.ReadTimeout = 30000;
            client.Config.DataConnectionConnectTimeout = 30000;
            client.Config.DataConnectionReadTimeout = 30000;

            await client.Connect();

            if (!await client.DirectoryExists(_fileStorageRemoteConfig.FtpDirectory))
            {
                await client.CreateDirectory(_fileStorageRemoteConfig.FtpDirectory);
            }

            string fullPath = $"{_fileStorageRemoteConfig.FtpDirectory}/{fileName}";

            _logger.LogInformation($"Ruta donde se guardará el archivo en el FTP: {fullPath}");

            using (var stream = file.OpenReadStream())
            {
                var result = await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);
                if (result != FtpStatus.Success)
                {
                    throw new Exception("Error al subir el archivo al servidor FTP.");
                }
            }

            await client.Disconnect();
            return fileName;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al guardar el archivo en el servidor FTP.");
        throw;
    }
}


    
public async Task<FileStream> GetFileFromFtpAsync(string fileName)
{
    _logger.LogInformation($"Intentando obtener el archivo desde FTP: {fileName}");

    try
    {
        using (var client = new AsyncFtpClient(
                   _fileStorageRemoteConfig.FtpHost, 
                   _fileStorageRemoteConfig.FtpUsername, 
                   _fileStorageRemoteConfig.FtpPassword))
        {
            client.Config.DataConnectionType = FtpDataConnectionType.AutoPassive;
            client.Config.ConnectTimeout = 30000; 
            client.Config.ReadTimeout = 30000;
            client.Config.DataConnectionConnectTimeout = 30000;
            client.Config.DataConnectionReadTimeout = 30000;

            await client.Connect();

            string remotePath = $"{_fileStorageRemoteConfig.FtpDirectory}/{fileName}";
            _logger.LogInformation($"Ruta del archivo en FTP: {remotePath}");

            if (!await client.FileExists(remotePath))
            {
                _logger.LogWarning($"Archivo no encontrado en el servidor FTP: {remotePath}");
                throw new FileStorageExceptions($"Archivo no encontrado: {fileName}");
            }

            _logger.LogInformation($"Archivo encontrado. Descargando...");

            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);

            FtpStatus status = await client.DownloadFile(tempFilePath, remotePath, FtpLocalExists.Overwrite);

            if (status != FtpStatus.Success)
            {
                throw new Exception($"Error al descargar el archivo {fileName} desde FTP.");
            }

            _logger.LogInformation($"Archivo descargado exitosamente: {tempFilePath}");

            return new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Delete);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener el archivo desde el FTP.");
        throw;
    }
}

    public async Task<FileStream> GettingMyDniPhotoFromFtpAsync()
    {
        _logger.LogInformation("Getting my DNI photo from FTP.");

        var user = _httpContextAccessor.HttpContext!.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var client = await _clientRepository.getByUserIdAsync(userId);
        if (client == null)
        {
            throw new ClientExceptions.ClientNotFoundException(userId);
        }

        string fileName = client.PhotoDni;

        if (string.IsNullOrEmpty(fileName) || fileName == "defaultDni.png")
        {
            fileName = "defaultDni.png";
        }

        try
        {
            _logger.LogInformation($"Resolving file path for DNI on FTP: {fileName}");
            var fileStream = await GetFileFromFtpAsync(fileName);

            _logger.LogInformation($"Returning DNI photo from FTP: {fileName}");
            return fileStream;
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning($"DNI photo not found on FTP for user: {userId}");
            throw new FileNotFoundException("DNI photo not found on FTP.");
        }
    }

    //
    public async Task<string> UpdateClientPhotoDniAsync(string clientId, IFormFile file)
    {
        _logger.LogInformation($"Request to update DNI photo for client: {clientId}");

        var client = await _clientRepository.GetByIdAsync(clientId);
        if (client == null)
        {
            _logger.LogError($"Client with ID {clientId} not found.");
            throw new ClientExceptions.ClientNotFoundException($"Client with ID {clientId} not found.");
        }

        _logger.LogInformation($"Client found: {client.Id}");
        _logger.LogInformation($"Fetching user with UserId: {client.UserId}");

        
        
        var user = await _userService.GetUserByIdAsync(client.UserId);
        
        
        string dni = user.Dni;
        string extension = Path.GetExtension(file.FileName).ToLower();
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        string fileName = $"DNI-{dni}-{timestamp}{extension}";

        _logger.LogInformation($"Generated file name: {fileName}");

        string savedFileName = await SaveFileToFtpAsync(file, fileName);

        _logger.LogInformation($"File uploaded successfully with filename: {savedFileName}");

        client.PhotoDni = savedFileName;
        await _clientRepository.UpdateAsync(client);

        _logger.LogInformation($"Client's DNI photo updated successfully.");
        return savedFileName;
    }
    
    public async Task<string> UpdateMyPhotoDniAsync(IFormFile file)
    {
        _logger.LogInformation("Updating DNI photo for current user.");

        var user = _httpContextAccessor.HttpContext!.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var client = await _clientRepository.getByUserIdAsync(userId);
        if (client == null)
        {
            _logger.LogError($"Client with user ID {userId} not found.");
            throw new ClientExceptions.ClientNotFoundException($"Client with user ID {userId} not found.");
        }

        var userData = await _userService.GetUserByIdAsync(client.UserId);
        if (userData == null)
        {
            _logger.LogError($"User with ID {client.UserId} not found.");
            throw new UserNotFoundException(client.UserId);
        }

        string dni = userData.Dni;
        string extension = Path.GetExtension(file.FileName).ToLower();
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        string fileName = $"DNI-{dni}-{timestamp}{extension}";

        _logger.LogInformation($"Generated file name for DNI: {fileName}");

        string savedFileName = await SaveFileToFtpAsync(file, fileName);

        client.PhotoDni = savedFileName;
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);

        _logger.LogInformation($"DNI photo updated successfully for user ID: {userId} (DNI: {dni})");

        return savedFileName;
    }



    
    public async Task<FileStream> GettingMyProfilePhotoAsync()
    {
        _logger.LogInformation("Getting my profile photo.");

        var user = _httpContextAccessor.HttpContext!.User;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var client = await _clientRepository.getByUserIdAsync(userId);
        if (client == null)
        {
            throw new ClientExceptions.ClientNotFoundException(userId);
        }

        string filePath = Path.Combine(_fileStorageConfig.UploadDirectory, client.Photo);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning($"Profile photo not found: {filePath}");
            throw new FileNotFoundException($"Profile photo file not found: {client.Photo}");
        }

        _logger.LogInformation($"Profile photo found: {filePath}");
        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    
    public async Task EnviarNotificacionUpdateAsync<T>(T t)
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userForFound = await _userService.GetUserByIdAsync(id);
        if (userForFound == null)
            throw new UserNotFoundException(id);
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Update.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };
        await _websocketHandler.NotifyUserAsync(userForFound.Id, notificacion);
    }

}