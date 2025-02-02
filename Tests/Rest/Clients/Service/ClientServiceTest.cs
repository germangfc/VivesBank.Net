using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentFTP;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Middleware.Jwt;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.storage.Config;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.WebSocket.Service;
using Path = System.IO.Path;
using Role = VivesBankApi.Rest.Users.Models.Role;

namespace Tests.Rest.Clients.Service;

using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Clients.Service;

public class ClientServiceTests
{
    private Mock<IClientRepository> _clientRepositoryMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<ILogger<ClientService>> _loggerMock;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<IDatabase> _cache;
    private Mock<IWebsocketHandler> _websocketHandlerMock;
    private Mock<IJwtGenerator> _jwtGenerator;
    private FileStorageConfig _fileStorageConfig;
    private ClientService _clientService;
    private Mock<AsyncFtpClient> _ftpClientMock;
    
    public ClientServiceTests()
    {
        _connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        _connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_cache.Object);

        _clientRepositoryMock = new Mock<IClientRepository>();
        _userServiceMock = new Mock<IUserService>();
        _loggerMock = new Mock<ILogger<ClientService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _websocketHandlerMock = new Mock<IWebsocketHandler>();
        _jwtGenerator = new Mock<IJwtGenerator>();
        _ftpClientMock = new Mock<AsyncFtpClient>();
        
        
        var inMemorySettings = new Dictionary<string, string>
        {
            { "FileStorageRemoteConfig:FtpHost", "127.0.0.1" },
            { "FileStorageRemoteConfig:FtpPort", "21" },
            { "FileStorageRemoteConfig:FtpUsername", "myuser" },
            { "FileStorageRemoteConfig:FtpPassword", "mypass" },
            { "FileStorageRemoteConfig:FtpDirectory", "/home/vsftpd" },
            { "FileStorageRemoteConfig:AllowedFileTypes:0", ".jpg" },
            { "FileStorageRemoteConfig:AllowedFileTypes:1", ".png" },
            { "FileStorageRemoteConfig:AllowedFileTypes:2", ".jpeg" },
            { "FileStorageRemoteConfig:MaxFileSize", "10485760" } 
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _fileStorageConfig = new FileStorageConfig();
        configuration.Bind("FileStorageRemoteConfig", _fileStorageConfig);
        
        _clientService = new ClientService(
            _loggerMock.Object,
            _userServiceMock.Object,
            _clientRepositoryMock.Object,
            _connection.Object,
            _httpContextAccessorMock.Object,
            _fileStorageConfig,
            _websocketHandlerMock.Object,
            _jwtGenerator.Object,
            configuration  
        );
    }
    
    
    [SetUp]
    public void Setup()
    {
        _httpContextAccessorMock.Reset();
        _userServiceMock.Reset();
        _clientRepositoryMock.Reset();
        _cache.Reset();
    }
    
    [TearDown]
    public void TearDown()
    {
        _httpContextAccessorMock.Reset();
        _userServiceMock.Reset();
        _clientRepositoryMock.Reset();
        _cache.Reset();
    }

    [Test]
    public async Task GetAllClientsAsync_ReturnsPagedClients()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;
        string fullName = "John";
        bool? isDeleted = false;
        string direction = "asc";

        var clients = new PagedList<Client>
        (
            new List<Client>
            {
                new Client { Id = "1", FullName = "John Doe", Adress = "Address 1", IsDeleted = false },
                new Client { Id = "2", FullName = "John Smith", Adress = "Address 2", IsDeleted = false }
            },
            totalCount: 2,
            pageNumber: pageNumber,
            pageSize: pageSize
        );

        _clientRepositoryMock
            .Setup(repo => repo.GetAllClientsPagedAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(clients);

        // Act
        var result = await _clientService.GetAllClientsAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(2, result.TotalCount);
        ClassicAssert.AreEqual(pageNumber, result.PageNumber);
        ClassicAssert.AreEqual(pageSize, result.PageSize);
        ClassicAssert.AreEqual("John Doe", result[0].Fullname);
        ClassicAssert.AreEqual("John Smith", result[1].Fullname);

        _clientRepositoryMock.Verify(repo =>
            repo.GetAllClientsPagedAsync(pageNumber, pageSize, fullName, isDeleted, direction), Times.Once);
    }

    [Test]
    public async Task GetAllClientsAsync_ReturnsEmptyList_WhenNoClientsFound()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 2;
        string fullName = "NonExistingName";
        bool? isDeleted = false;
        string direction = "asc";

        var clients = new PagedList<Client>(new List<Client>(), totalCount: 0, pageNumber: pageNumber, pageSize: pageSize);

        _clientRepositoryMock
            .Setup(repo => repo.GetAllClientsPagedAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(clients);

        // Act
        var result = await _clientService.GetAllClientsAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(0, result.TotalCount);
        ClassicAssert.AreEqual(pageNumber, result.PageNumber);
        ClassicAssert.AreEqual(pageSize, result.PageSize);

        _clientRepositoryMock.Verify(repo =>
            repo.GetAllClientsPagedAsync(pageNumber, pageSize, fullName, isDeleted, direction), Times.Once);
    }

    [Test]
    public async Task GetMyClientData_ShouldReturn_ClientResponse()
    {
        var userId = "user1";
        var clientId = "client1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);

        var client = new Client { Id = clientId, UserId = userId };
        _clientRepositoryMock.Setup(repo => repo.getByUserIdAsync(userId)).ReturnsAsync(client);

        var result = await _clientService.GettingMyClientData();
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(clientId, result.Id);
        ClassicAssert.AreEqual(userId, result.UserId);
        
        _userServiceMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
        _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task GetMyClientData_ShouldReturn_UserNotFoundException()
    {
        var userId = "user1";
        var clientId = "client1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ThrowsAsync(new UserNotFoundException(userId));
        
       var result = Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.GettingMyClientData());
       
       ClassicAssert.AreEqual($"The user with id: {userId} was not found", result.Message);
       
       _userServiceMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
       _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetMyClientData_ShouldReturn_ClientNotFoundException()
    {
        var userId = "user1";
        var clientId = "client1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);

        _clientRepositoryMock.Setup(repo => repo.getByUserIdAsync(userId)).ThrowsAsync(new ClientExceptions.ClientNotFoundException(clientId));

       var result = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.GettingMyClientData());
       
       ClassicAssert.AreEqual($"Client not found by id {clientId}", result.Message);
       
       _userServiceMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
       _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(userId), Times.Once);
    }
    
    [Test]
    public async Task GetClientByIdAsync_ReturnsClient()
    {
        // Arrange
        var clientId = "1";
        var client = new Client { Id = clientId, FullName = "John Doe", Adress = "Address 1", IsDeleted = false };
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(client);

        // Act
        var result = await _clientService.GetClientByIdAsync(clientId);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(clientId, result.Id);
        ClassicAssert.AreEqual("John Doe", result.Fullname);
        ClassicAssert.AreEqual("Address 1", result.Address);
        ClassicAssert.IsFalse(result.IsDeleted);

        _clientRepositoryMock.Verify(repo => repo.GetByIdAsync(clientId), Times.Once);
    }
    
    [Test]
    public async Task GetClientByIdAsync_ReturnsClient_WhenInCache()
    {
        // Arrange
        var clientId = "1";
        var client = new Client { Id = clientId, FullName = "John Doe", Adress = "Address 1", IsDeleted = false };
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonConvert.SerializeObject(client));

        // Act
        var result = await _clientService.GetClientByIdAsync(clientId);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("1", result.Id);
        ClassicAssert.AreEqual("John Doe", result.Fullname);
        ClassicAssert.AreEqual("Address 1", result.Address);
        ClassicAssert.IsFalse(result.IsDeleted);
        
        _cache.Verify(repo => repo.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public void GetClientByIdAsync_ThrowsClientNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = "1";
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync((Client)null);

        // Act & Assert
        Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.GetClientByIdAsync(clientId));
    }

    [Test]
    public async Task CreateClientAsync_ShouldReturn_Token()
    {
        
        var userId = "validId";
        var request = new ClientRequest
        {
            FullName = "John Doe",
            Address = "Address 1",
        };
        var user = new User
        {
            Id = userId,
            Dni = "12345678Z",
            Password = "calamarDelNorte123",
            Role = Role.User
        };
        var userUpdateRequest = new UserUpdateRequest { Role = Role.Client.ToString() };
        
        var client = new Client { Id = "client-id", UserId = userId };

        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var userFound = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user.ToUserResponse());

        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync((Client)null);
        
        _userServiceMock.Setup(x => x.UpdateUserAsync(userId, userUpdateRequest))
            .ReturnsAsync(new UserResponse { Id = userId, Role = Role.Client.ToString() });
        _clientRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Client>()))
            .Returns(Task.CompletedTask);
        
        var jwtToken = "generatedJwtToken";
        _jwtGenerator.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
            .Returns(jwtToken);
        
        var result = await _clientService.CreateClientAsync(request);
        
        ClassicAssert.AreEqual(jwtToken, result);
        _userServiceMock.Verify(x => x.UpdateUserAsync(userId, It.IsAny<UserUpdateRequest>()), Times.Once);
        _clientRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Client>()), Times.Once);
    }
    
    [Test]
    public async Task CreateClientAsync_ShouldReturn_UserNotFoundException()
    {
        
        var userId = "user1";
        var clientId = "client1";
        var request = new ClientRequest
        {
            FullName = "John Doe",
            Address = "Address 1",
        };
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ThrowsAsync(new UserNotFoundException(userId));
        
        var result = Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.CreateClientAsync(request));
       
        ClassicAssert.AreEqual($"The user with id: {userId} was not found", result.Message);
       
        _userServiceMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
        _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CreateClientAsync_ShouldReturn_ClientAlreadyExistsException()
    {
        
        var userId = "user1";
        var clientId = "client1";
        var request = new ClientRequest
        {
            FullName = "John Doe",
            Address = "Address 1",
        };
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);

        var existingClient = new Client { UserId = userId };
        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync(existingClient);
        
        var result = Assert.ThrowsAsync<ClientExceptions.ClientAlreadyExistsException>(() => _clientService.CreateClientAsync(request));
        
        ClassicAssert.AreEqual($"A client already exists with this user id {userId}", result.Message);
        
        _userServiceMock.Verify(u => u.GetUserByIdAsync(userId), Times.Once);
        _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(userId), Times.Once);
    }


    [Test]
    public async Task CreateClient_WithAExistingUser_ShouldReturnException()
    {
        var clientRequest = new ClientRequest();
        var userId = "user123";
        
        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(c => c.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(claimsPrincipalMock.Object);

       
        var userForFound = new User { Id = userId };
        var userResponse = new UserResponse
        {
            Id = "random",
            Dni = userForFound.Id,
            Role = Role.User.ToString(),
            CreatedAt = DateTime.UtcNow,
        };
        _userServiceMock.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(userResponse);

        
        var existingClient = new Client { UserId = userId };
        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync(existingClient);
        
        var result = Assert.ThrowsAsync<ClientExceptions.ClientAlreadyExistsException>(
            () => _clientService.CreateClientAsync(clientRequest)
        );
        
        Assert.That(result.Message, Is.EqualTo($"A client already exists with this user id {userId}"));
        
    }

    [Test]
    public void CreateClient_NonExistingUser()
    {
        // Arrange
        var clientRequest = new ClientRequest();
        var userId = "user123";

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(c => c.FindFirst(ClaimTypes.NameIdentifier)).Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        _httpContextAccessorMock.Setup(h => h.HttpContext.User).Returns(claimsPrincipalMock.Object);

        _userServiceMock.Setup(repo => repo.GetUserByIdAsync(userId)).ThrowsAsync(new UserNotFoundException(userId));

        // Act & Assert
        var result = Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.CreateClientAsync(clientRequest));
        Assert.That(result.Message, Is.EqualTo($"The user with id: {userId} was not found"));
    }
    
    [Test]
    public async Task CreateClient_UserNotFound_ThrowsUserNotFoundException()
    {
        // Arrange
        var clientRequest = new ClientRequest();
        var userId = "user123";

        var claimsPrincipalMock = new Mock<ClaimsPrincipal>();
        claimsPrincipalMock.Setup(c => c.FindFirst(ClaimTypes.NameIdentifier))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));
        _httpContextAccessorMock.Setup(h => h.HttpContext.User)
            .Returns(claimsPrincipalMock.Object);

        _userServiceMock.Setup(repo => repo.GetUserByIdAsync(userId))
            .ReturnsAsync((UserResponse)null); // Simula que el usuario no existe

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(
            async () => await _clientService.CreateClientAsync(clientRequest)
        );

        Assert.That(ex.Message, Is.EqualTo($"The user with id: {userId} was not found"));
    }

    

    [Test]
    public void UpdateClientAsync_ThrowsClientNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = "1";
        var updateRequest = new ClientUpdateRequest { FullName = "Updated Name", Address = "Updated Address" };
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync((Client)null);

        // Act & Assert
        Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.UpdateClientAsync(clientId, updateRequest));
    }

    [Test]
    public async Task UpdateClientAsync_UpdatesClient()
    {
        // Arrange
        var clientId = "1";
        var updateRequest = new ClientUpdateRequest { FullName = "Updated Name", Address = "Updated Address" };
        var existingClient = new Client { Id = clientId, FullName = "Old Name", Adress = "Old Address" };

        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(existingClient);

        // Act
        await _clientService.UpdateClientAsync(clientId, updateRequest);

        // Assert
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.FullName == "Updated Name" && c.Adress == "Updated Address")), Times.Once);
    }
    
    [Test]
    public async Task UpdateClientAsync_UpdatesClient_WhenFoundInCache()
    {
        // Arrange
        var clientId = "1";
        var updateRequest = new ClientUpdateRequest { FullName = "Updated Name", Address = "Updated Address" };
        var existingClient = new Client { Id = clientId, FullName = "Old Name", Adress = "Old Address" };

        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonConvert.SerializeObject(existingClient));

        // Act
        await _clientService.UpdateClientAsync(clientId, updateRequest);

        // Assert
        _cache.Verify(repo => repo.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Test]
    public async Task UpdateMyClientData_ShouldReturn_ClientResponse()
    {
        var userId = "user1";
        var clientId = "client1";
        var request = new ClientUpdateRequest
        {
            FullName = "Simeone",
            Address = "Calderon",
        };
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        
        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);

        var client = new Client { Id = clientId, UserId = userId, FullName = "guliano", Adress = "wanda"};
        _clientRepositoryMock.Setup(repo => repo.getByUserIdAsync(userId)).ReturnsAsync(client);
        
        var result = await _clientService.UpdateMeAsync(request);
        
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("Calderon", result.Address);
        ClassicAssert.AreEqual("Simeone", result.Fullname);

        
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.Adress =="Calderon"&& c.FullName == "Simeone")), Times.Once);
    }

    [Test]
    public void UpdateMyClientData_ShouldReturn_ClientNotfoundException()
    {
        // Arrange
        var userId = "user1";
        var clientId = "client1";
        var request = new ClientUpdateRequest
        {
            FullName = "Simeone",
            Address = "Calderon",
        };
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);
        
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(new UserResponse { Id = userId });

        _clientRepositoryMock.Setup(repo => repo.getByUserIdAsync(userId)).ReturnsAsync((Client)null);
        
        var result = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.UpdateMeAsync(request));
        
        ClassicAssert.AreEqual($"Client not found by id {userId}", result.Message);
        _clientRepositoryMock.Verify(repo => repo.getByUserIdAsync(userId), Times.Once);
    }
    [Test]
    public void LogicDeleteClientAsync_ThrowsClientNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = "1";
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync((Client)null);

        // Act & Assert
        Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.LogicDeleteClientAsync(clientId));
    }

    [Test]
    public async Task LogicDeleteClientAsync_SetsClientIsDeleted()
    {
        // Arrange
        var clientId = "1";
        var existingClient = new Client { Id = clientId, IsDeleted = false };
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(existingClient);

        // Act
        await _clientService.LogicDeleteClientAsync(clientId);

        // Assert
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.IsDeleted == true)), Times.Once);
    }

    [Test]
    public async Task DeleteMeAsClient_ShouldLogicDeleteClient()
    {
        var userId = "user1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        var client = new Client { Id = "client1", UserId = userId, IsDeleted = false };

        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync(client);
        
        await _clientService.DeleteMe();
        
        ClassicAssert.IsTrue(client.IsDeleted);
        _clientRepositoryMock.Verify(c => c.UpdateAsync(client), Times.Once); 
        _userServiceMock.Verify(u => u.DeleteUserAsync(userId,  true), Times.Once);
    }

    [Test]
    public void DeleteMeAsClient_ShouldReturn_ClientNotfoundException()
    {
        var userId = "user1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(new UserResponse { Id = userId });
        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync((Client)null);
        
        var result = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.DeleteMe());
        
        ClassicAssert.AreEqual($"Client not found by id {userId}", result.Message);
        _clientRepositoryMock.Verify(c => c.getByUserIdAsync(userId), Times.Once);
    }

    [Test]
    public void DeleteMeAsClient_ShouldReturn_UserNotFoundException()
    {
        var userId = "user1";
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));
        var httpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContext);

        var user = new UserResponse { Id = userId };
        _userServiceMock.Setup(u => u.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(c => c.getByUserIdAsync(userId)).ReturnsAsync(new Client { Id = "client1", UserId = userId, IsDeleted = false });
        
        _userServiceMock.Setup(u => u.DeleteUserAsync(userId, true)).ThrowsAsync(new UserNotFoundException(userId));
        
        var result =  Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.DeleteMe());
    }
    

    
    
    
    [Test]
    public void SaveFileAsync_FileExceedsMaxSize_ThrowsFileStorageException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(_fileStorageConfig.MaxFileSize + 1);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act & Assert
        var ex = Assert.ThrowsAsync<FileStorageExceptions>(
            async () => await _clientService.SaveFileAsync(fileMock.Object, "baseFile")
        );

        Assert.That(ex.Message, Is.EqualTo("El tamaño del fichero excede del máximo permitido"));
    }
    
    [Test]
    public void SaveFileAsync_FileTypeNotAllowed_ThrowsFileStorageException()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(500);
        fileMock.Setup(f => f.FileName).Returns("test.exe"); // Tipo no permitido

        // Act & Assert
        var ex = Assert.ThrowsAsync<FileStorageExceptions>(
            async () => await _clientService.SaveFileAsync(fileMock.Object, "baseFile")
        );

        Assert.That(ex.Message, Is.EqualTo("Tipo de fichero no permitido"));
    }
    
    [Test]
    public async Task SaveFileAsync_ValidFile_ReturnsFileName()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1 KB file
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var baseFileName = "user-file";

        // Act
        var result = await _clientService.SaveFileAsync(fileMock.Object, baseFileName);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result.Contains(baseFileName));
        ClassicAssert.IsTrue(result.EndsWith(".jpg"));
    }
    
    [Test]
    public async Task SaveFileAsync_DirectoryDoesNotExist_CreatesDirectory()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024); // 1 KB file
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var baseFileName = "user-file";
        var uploadPath = Path.Combine(_fileStorageConfig.UploadDirectory);

        // Ensure the directory does not exist initially
        if (Directory.Exists(uploadPath))
        {
            Directory.Delete(uploadPath, true);
        }

        // Act
        await _clientService.SaveFileAsync(fileMock.Object, baseFileName);

        // Assert
        ClassicAssert.IsTrue(Directory.Exists(uploadPath));
    }
    
    [Test]
    public async Task UpdateClientDniPhotoAsync_FileNotFoundException()
    {
        // Arrange
        string clientId = "123";

        // Act & Assert
        Assert.ThrowsAsync<FileNotFoundException>(() => _clientService.UpdateClientDniPhotoAsync(clientId, null));
    }
    
    
    [Test]
    public async Task UpdateClientDniPhotoAsync_ShouldThrowClientNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        string clientId = "123";
        
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("dummy content");
        writer.Flush();
        stream.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("dni-photo.png");

        // Simulamos que el cliente no existe
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId))
            .ReturnsAsync((Client)null);

        // Act & Assert
        Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(
            () => _clientService.UpdateClientDniPhotoAsync(clientId, fileMock.Object)
        );
    }
    
    [Test]
    public async Task UpdateClientDniPhotoAsync_ShouldThrowUserNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var clientId = "123";
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("dummy content");
        writer.Flush();
        stream.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("dni-photo.png");

        var client = new Client { Id = clientId, UserId = "user1" };

        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(client);
        _userServiceMock.Setup(service => service.GetUserByIdAsync(client.UserId)).ReturnsAsync((UserResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.UpdateClientDniPhotoAsync(clientId, fileMock.Object));
        Assert.That(ex.Message, Is.EqualTo($"The user with id: {client.UserId} was not found"));
    }
    
    [Test]
    public async Task UpdateClientDniPhotoAsync_ShouldUpdateDniPhotoSuccessfully()
    {
        // Arrange
        var clientId = "123";
        var userId = "user1";
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("dummy content");
        writer.Flush();
        stream.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("dni-photo.png");

        var client = new Client { Id = clientId, UserId = userId, PhotoDni = "old-dni-photo.png" };
        var user = new UserResponse { Id = userId, Dni = "12345678X" };

        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(client);
        _userServiceMock.Setup(service => service.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);

        // Act
        var result = await _clientService.UpdateClientDniPhotoAsync(clientId, fileMock.Object);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result.Contains("DNI-12345678X"));
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.PhotoDni == result)), Times.Once);
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.UpdatedAt != null)), Times.Once);
    }
    
 
    [Test]
    public async Task UpdateClientPhotoAsync_ShouldUpdatePhotoSuccessfully()
    {
        // Arrange
        var clientId = "123";
        var userId = "user1";
        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("dummy content");
        writer.Flush();
        stream.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.FileName).Returns("profile-photo.png");

        var client = new Client { Id = clientId, UserId = userId, Photo = "old-photo.png" };
        var user = new UserResponse { Id = userId, Dni = "12345678X" };

        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(client);
        _userServiceMock.Setup(service => service.GetUserByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Client>())).Returns(Task.CompletedTask);

        // Act
        var result = await _clientService.UpdateClientPhotoAsync(clientId, fileMock.Object);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.IsTrue(result.Contains("PROFILE-12345678X"));
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.Photo == result)), Times.Once);
        _clientRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Client>(c => c.UpdatedAt != null)), Times.Once);
    }
    
    [Test]
    public void UpdateClientPhotoAsync_ShouldThrowFileNotFoundException_WhenFileIsNull()
    {
        // Arrange
        var clientId = "123";
        IFormFile file = null;

        // Act & Assert
        var ex = Assert.ThrowsAsync<FileNotFoundException>(() => _clientService.UpdateClientPhotoAsync(clientId, file));
        Assert.That(ex.Message, Is.EqualTo("No file was provided or the file is empty."));
    }

    [Test]
    public void UpdateClientPhotoAsync_ShouldThrowFileNotFoundException_WhenFileIsEmpty()
    {
        // Arrange
        var clientId = "123";
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act & Assert
        var ex = Assert.ThrowsAsync<FileNotFoundException>(() => _clientService.UpdateClientPhotoAsync(clientId, fileMock.Object));
        Assert.That(ex.Message, Is.EqualTo("No file was provided or the file is empty."));
    }
    
    [Test]
    public void UpdateClientPhotoAsync_ShouldThrowClientNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var clientId = "123";
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync((Client)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.UpdateClientPhotoAsync(clientId, fileMock.Object));
        Assert.That(ex.Message, Is.EqualTo($"Client not found by id Client with ID {clientId} not found."));
    }
    
    [Test]
    public void UpdateClientPhotoAsync_ShouldThrowUserNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var clientId = "123";
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        var client = new Client { Id = clientId, UserId = "user1" };
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync(client);
        _userServiceMock.Setup(service => service.GetUserByIdAsync(client.UserId)).ReturnsAsync((UserResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(() => _clientService.UpdateClientPhotoAsync(clientId, fileMock.Object));
        Assert.That(ex.Message, Is.EqualTo($"The user with id: {client.UserId} was not found"));
    }
    
    
    [Test]
    public async Task GetFileAsync_ShouldReturnFileStream_WhenFileExists()
    {
        // Arrange
        var fileName = "existing-file.txt";
        var filePath = Path.Combine(_fileStorageConfig.UploadDirectory, fileName);
        var fileContent = "dummy content";
        await File.WriteAllTextAsync(filePath, fileContent);

        // Act
        var result = await _clientService.GetFileAsync(fileName);

        // Assert
        ClassicAssert.IsNotNull(result);
        using (var reader = new StreamReader(result))
        {
            var content = await reader.ReadToEndAsync();
            ClassicAssert.AreEqual(fileContent, content);
        }

        // Cleanup
        result.Dispose();
        File.Delete(filePath);
    }
    
    [Test]
    public void GetFileAsync_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var fileName = "non-existing-file.txt";

        // Act & Assert
        var ex = Assert.ThrowsAsync<FileNotFoundException>(() => _clientService.GetFileAsync(fileName));
        Assert.That(ex.Message, Is.EqualTo($"File not found: {fileName}"));
    }
    

    [Test]
    public void UpdateClientPhotoDniAsync_ShouldThrowClientNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var clientId = "123";
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        _clientRepositoryMock.Setup(repo => repo.GetByIdAsync(clientId)).ReturnsAsync((Client)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(() => _clientService.UpdateClientPhotoDniAsync(clientId, fileMock.Object));
        Assert.That(ex.Message, Is.EqualTo($"Client not found by id El cliente con ClientId {clientId} no existe."));
      
    [Test]
    public async Task Export()
    {
        //Arrange
        var client = new Client { Id = "1", FullName = "John Doe", Adress = "Address 1", IsDeleted = false };

        //Act
        var result = await _clientService.ExportOnlyMeData(client);

        //Assert
        var formFile = new FormFile(result, 0, result.Length, "something", "test.json"  );
        var secondResult = await _clientService.Import(formFile).ToList();
        ClassicAssert.IsInstanceOf<FileStream>(result);
        ClassicAssert.AreEqual(client.Id, secondResult[0].Id);
        ClassicAssert.AreEqual(client.FullName, secondResult[0].FullName);
        ClassicAssert.AreEqual(client.UserId, secondResult[0].UserId);
        ClassicAssert.AreEqual(client.Adress, secondResult[0].Adress);
        
        //Clean up
        await result.DisposeAsync();
    }
    
}
