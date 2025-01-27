using System.Security.Claims;
using FluentFTP;
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
        IConfiguration configuration
        )
    {
        _userRepository = userRepository; 
        _logger = logger;
        _clientRepository = clientRepository;
        _cache = connection.GetDatabase();
        _httpContextAccessor = httpContextAccessor;
        _fileStorageConfig = fileStorageConfig;
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
    
    public async Task<string> SaveFileToFtpAsync(IFormFile file, string fileName)
{
    try
    {
        // Validar que la configuración no sea nula
        if (_fileStorageRemoteConfig == null)
        {
            _logger.LogError("La configuración de almacenamiento remoto (_fileStorageRemoteConfig) es nula.");
            throw new InvalidOperationException("La configuración de almacenamiento remoto no está disponible.");
        }

        // Establecer el host FTP dependiendo del entorno
        string ftpHost = _fileStorageRemoteConfig.FtpHost;

        // Si estamos fuera de Docker, usa 'localhost'
        if (string.IsNullOrEmpty(ftpHost))
        {
            ftpHost = "localhost"; // O "127.0.0.1" si prefieres
        }

        // Validar que las propiedades necesarias estén configuradas
        if (string.IsNullOrEmpty(ftpHost) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpUsername) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpPassword) ||
            string.IsNullOrEmpty(_fileStorageRemoteConfig.FtpDirectory))
        {
            _logger.LogError("La configuración de almacenamiento remoto contiene valores nulos o vacíos.");
            throw new InvalidOperationException("La configuración de almacenamiento remoto es inválida.");
        }

        // Validar el tipo de archivo
        string extension = Path.GetExtension(file.FileName).ToLower();
        if (!_fileStorageRemoteConfig.AllowedFileTypes.Contains(extension))
        {
            throw new InvalidOperationException($"El tipo de archivo '{extension}' no está permitido.");
        }

        // Validar el tamaño del archivo
        if (file.Length > _fileStorageRemoteConfig.MaxFileSize)
        {
            throw new InvalidOperationException($"El archivo excede el tamaño máximo permitido de {_fileStorageRemoteConfig.MaxFileSize} bytes.");
        }

        // Conexión al FTP
        using (var client = new AsyncFtpClient(ftpHost, _fileStorageRemoteConfig.FtpUsername, _fileStorageRemoteConfig.FtpPassword))
        {
            _logger.LogInformation("Intentando conectar al servidor FTP...");

            // Configurar tiempos de espera
            client.Config.ConnectTimeout = 30000;  // Tiempo de espera al conectar (30s)
            client.Config.ReadTimeout = 30000;     // Tiempo de espera para lectura (30s)
            client.Config.DataConnectionConnectTimeout = 30000; // Tiempo de espera para conexiones de datos (30s)
            client.Config.DataConnectionReadTimeout = 30000;    // Tiempo de espera para transferencias (30s)


            // Conexión al servidor FTP
            await client.Connect();
            _logger.LogInformation("Conexión exitosa al servidor FTP.");

            // Crear directorio si no existe
            if (!await client.DirectoryExists(_fileStorageRemoteConfig.FtpDirectory))
            {
                _logger.LogWarning($"El directorio '{_fileStorageRemoteConfig.FtpDirectory}' no existe. Creándolo...");
                await client.CreateDirectory(_fileStorageRemoteConfig.FtpDirectory);
                _logger.LogInformation($"Directorio '{_fileStorageRemoteConfig.FtpDirectory}' creado exitosamente.");
            }
            else
            {
                _logger.LogInformation($"El directorio '{_fileStorageRemoteConfig.FtpDirectory}' ya existe.");
            }

            // Construir la ruta completa del archivo en el servidor
            string fullPath = $"{_fileStorageRemoteConfig.FtpDirectory}/{fileName}";
            _logger.LogInformation($"Ruta completa del archivo remoto: {fullPath}");

            // Subir el archivo al servidor
            using (var stream = file.OpenReadStream())
            {
                _logger.LogInformation($"Subiendo archivo '{fileName}' al servidor FTP...");
                var uploadStatus = await client.UploadStream(stream, fullPath, FtpRemoteExists.Overwrite, true);

                if (uploadStatus != FtpStatus.Success)
                {
                    throw new Exception($"Error al subir el archivo. Estado de subida: {uploadStatus}");
                }
            }

            _logger.LogInformation($"Archivo '{fileName}' subido exitosamente al servidor FTP.");
            await client.Disconnect();
            _logger.LogInformation("Desconexión exitosa del servidor FTP.");

            return fullPath;  // Devolvemos la ruta completa del archivo en el servidor FTP
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al guardar el archivo en el servidor FTP.");
        throw;  // Relanzar la excepción para que sea manejada en un nivel superior
    }
}





    public async Task<FileStream> GetFileFromFtpAsync(string fileName)
    {
        _logger.LogInformation($"Getting file from FTP: {fileName}");

        try
        {
            using (var client = new AsyncFtpClient(_fileStorageRemoteConfig.FtpHost, _fileStorageRemoteConfig.FtpUsername, _fileStorageRemoteConfig.FtpPassword))
            {
                await client.Connect();

                string remotePath = $"{_fileStorageRemoteConfig.FtpDirectory}/{fileName}";

                if (!await client.FileExists(remotePath))
                {
                    _logger.LogWarning($"File not found on FTP server: {remotePath}");
                    throw new FileStorageExceptions($"File not found: {fileName}");
                }

                _logger.LogInformation($"File found on FTP server: {remotePath}");

                string tempFilePath = Path.GetTempFileName();

                await client.DownloadFile(tempFilePath, remotePath, FtpLocalExists.Overwrite);

                _logger.LogInformation($"File downloaded to temporary path: {tempFilePath}");

                return new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Delete);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file from FTP");
            throw;
        }
    }

    public async Task DeleteFileFromFtpAsync(string fileName)
    {
        _logger.LogInformation("Deleting file from FTP {fileName}", fileName);
    
        try
        {
            using (var client = new AsyncFtpClient(_fileStorageRemoteConfig.FtpHost, _fileStorageRemoteConfig.FtpUsername, _fileStorageRemoteConfig.FtpPassword))
            {
                await client.Connect();

                // Construye la ruta completa en el servidor
                string remotePath = $"{_fileStorageRemoteConfig.FtpDirectory}/{fileName}";

                // Verifica si el archivo existe en el servidor
                if (!await client.FileExists(remotePath))
                {
                    _logger.LogWarning($"File not found on FTP server: {remotePath}");
                    throw new FileNotFoundException($"El archivo '{fileName}' no existe en el servidor FTP.");
                }
                _logger.LogInformation($"File found on FTP server: {remotePath}");

                // Elimina el archivo
                await client.DeleteFile(remotePath);

                await client.Disconnect();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from FTP");
            throw;
        }
    }

    
    public async Task<string> UpdateClientPhotoDniAsync(string clientId, IFormFile file)
    {
        try
        {
            // Verificar si el cliente existe usando el ClientId
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                throw new ClientExceptions.ClientNotFoundException($"El cliente con ClientId {clientId} no existe.");
            }

            // Buscar el usuario para obtener el DNI
            var user = await _userRepository.GetByIdAsync(client.UserId); // Aquí usamos el UserId del cliente
            if (user == null)
            {
                throw new UserNotFoundException($"El usuario con UserId {client.UserId} no existe.");
            }

            string dni = user.Dni;

            // Generar el nombre del archivo basado en el DNI y el timestamp
            string extension = Path.GetExtension(file.FileName).ToLower();
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            string fileName = $"{dni}-{timestamp}{extension}";

            // Guardar el archivo en el servidor FTP y obtener el nombre del archivo almacenado
            string savedFileName = await SaveFileToFtpAsync(file, fileName);

            // Actualizar la propiedad de la foto del cliente con la nueva URL
            client.PhotoDni = savedFileName;

            // Actualizar el cliente en la base de datos
            await _clientRepository.UpdateAsync(client);

            return savedFileName;  // O puedes devolver la URL completa del archivo si es necesario
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la foto del cliente");
            throw;
        }
    }



}