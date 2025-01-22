using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Users.Models;
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
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<IDatabase> _cache;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<ClientService>> _loggerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly ClientService _clientService;
    
    public ClientServiceTests()
    {
        _connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        _connection.Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_cache.Object);
        _clientRepositoryMock = new Mock<IClientRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<ClientService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _clientService = new ClientService(_loggerMock.Object, _userRepositoryMock.Object, _clientRepositoryMock.Object, _connection.Object, _httpContextAccessorMock.Object);
    }
    
    [TearDown]
    public void TearDown()
    {
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
    public async Task CreateClientAsync_ShouldReturn()
    {
        // Arrange
        var userId = "validId";
        var request = new ClientRequest
        {
            FullName = "John Doe",
            Address = "Address 1",
        };
        var user = new User
        {
            Id = userId,
            Password = "calamarDelNorte123",
            Role = Role.User
        };
        var client = new Client { Id = "client-id", UserId = userId };

        // Simula el contexto HTTP con el usuario autenticado
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));
        var mockHttpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(mockHttpContext);

        // Configura los mocks del repositorio
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Client>())).Callback<Client>(c =>
        {
            c.Id = client.Id;
            c.UserId = client.UserId;
        });

        // Act
        var result = await _clientService.CreateClientAsync(request);

        // Assert
        _clientRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Client>()), Times.Once);
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(client.Id, result.Id);
        ClassicAssert.AreEqual(client.UserId, result.UserId);
    }

    [Test]
    public async Task CreateClient_WithAExistingUser_ShouldReturnException()
    {
        var userId = "existing_user";
        var request = new ClientRequest
        {
            FullName = "John Doe",
            Address = "Address 1",
        };
        var user = new User
        {
            Id = userId,
            Password = "calamarDelNorte123",
            Role = Role.User
        };
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));
        var mockHttpContext = new DefaultHttpContext { User = claims };
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(mockHttpContext);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
        _clientRepositoryMock.Setup(repo => repo.getByUserIdAsync(It.IsAny<String>())).ReturnsAsync((Client?)null);
        
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
}
