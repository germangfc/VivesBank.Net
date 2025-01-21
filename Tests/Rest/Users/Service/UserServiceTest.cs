using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.WebSocket.Service;
using Role = VivesBankApi.Rest.Users.Models.Role;

namespace Tests.Rest.Users.Service;

[TestFixture]
public class UserServiceTest
{
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<IDatabase> _cache;
    private Mock<IUserRepository> userRepositoryMock;
    private Mock<AuthJwtConfig> _authConfig;
    private Mock<ILogger<UserService>> _logger;
    private UserService userService;
    private User _user1;
    private User _user2;
    private Mock<WebSocketHandler> _webSocketHandler;
    private Mock<IHttpContextAccessor> _httpContextAccessor;

    [SetUp]
    public void SetUp()
    {
        _connection = new Mock<IConnectionMultiplexer>();
        _cache = new Mock<IDatabase>();
        _connection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<string>())).Returns(_cache.Object);
        
        userRepositoryMock = new Mock<IUserRepository>();

        userService = new UserService(_logger.Object, userRepositoryMock.Object, _authConfig.Object,_connection.Object, _webSocketHandler.Object, _httpContextAccessor.Object);
        
        _user1 = new User
        {
            Id = "1",
            Dni = "43080644B",
            Password = "Password123",
            Role = Role.Admin,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        _user2 = new User
        {
            Id = "2",
            Dni = "86896998P",
            Password = "SecurePass456",
            Role = Role.User,
            CreatedAt = DateTime.Now.AddDays(-1),
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };
    }

    [Test]
    public async Task GetAllUsersAsync()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string role = "Admin";
        bool? isDeleted = false;
        string direction = "asc";

        var usersFromRepo = new PagedList<User>(
            new List<User> { _user1, _user2 },
            2,
            pageNumber,
            pageSize
        );

        userRepositoryMock
            .Setup(repo => repo.GetAllUsersPagedAsync(pageNumber, pageSize, role, isDeleted, direction))
            .ReturnsAsync(usersFromRepo);

        // Act
        var result = await userService.GetAllUsersAsync(pageNumber, pageSize, role, isDeleted, direction);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(2, result.TotalCount);
        ClassicAssert.AreEqual(pageNumber, result.PageNumber);
        ClassicAssert.AreEqual(pageSize, result.PageSize);
        ClassicAssert.AreEqual(2, result.Count);
        ClassicAssert.AreEqual("43080644B", result.First().Dni);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersFound()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string role = "Admin";
        bool? isDeleted = false;
        string direction = "asc";

        var usersFromRepo = new PagedList<User>(
            new List<User>(),
            0, // TotalCount
            pageNumber, // PageNumber
            pageSize // PageSize
        );

        userRepositoryMock
            .Setup(repo => repo.GetAllUsersPagedAsync(pageNumber, pageSize, role, isDeleted, direction))
            .ReturnsAsync(usersFromRepo);

        // Act
        var result = await userService.GetAllUsersAsync(pageNumber, pageSize, role, isDeleted, direction);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(0, result.TotalCount);
        ClassicAssert.AreEqual(result.TotalCount, 0);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsMappedUserResponse()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string role = "User";
        bool? isDeleted = false;
        string direction = "asc";

        var usersFromRepo = new PagedList<User>(
            new List<User> { _user1 },
            1,
            pageNumber,
            pageSize
        );
        
        userRepositoryMock
            .Setup(repo => repo.GetAllUsersPagedAsync(pageNumber, pageSize, role, isDeleted, direction))
            .ReturnsAsync(usersFromRepo);

        // Act
        var result = await userService.GetAllUsersAsync(pageNumber, pageSize, role, isDeleted, direction);

        // Assert
        var userResponse = result.First();
        ClassicAssert.AreEqual("43080644B", userResponse.Dni);
        ClassicAssert.AreEqual(Role.Admin.ToString(), userResponse.Role);
        ClassicAssert.False(userResponse.IsDeleted);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WhenInCache()
    {
        // Arrange
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonSerializer.Serialize(_user1));

        // Act
        var result = await userService.GetUserByIdAsync(_user1.Id);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_user1.Dni, result.Dni);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(_user1.Id), Times.Never);
    }
    
    [Test]
    public async Task GetUserByIdAsync_WhenNotInCache()
    {
        // Arrange
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(_user1.Id)).ReturnsAsync(_user1);

        // Act
        var result = await userService.GetUserByIdAsync(_user1.Id);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_user1.Dni, result.Dni);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(_user1.Id), Times.Once);
    }
    
    [Test]
    public void GetUserByIdAsync_NotExist()
    {
        // Arrange
        userRepositoryMock.Setup(repo => repo.GetByIdAsync("3")).ReturnsAsync((User)null);

        // Act
        Assert.ThrowsAsync<UserNotFoundException>((() => userService.GetUserByIdAsync("3")));

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync("3"), Times.Once);
    }
    
    [Test]
    public async Task AddUserAsync()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Dni = "43080644B",
            Password = "Password123",
            Role = "Admin"
        };

        var newUser = userRequest.toUser();
        
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userRequest.Dni)).ReturnsAsync(_user2);
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userRequest.Dni)).ReturnsAsync((User)null);
        userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await userService.AddUserAsync(userRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(newUser.Dni, result.Dni);
            ClassicAssert.AreEqual("Admin", result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userRequest.Dni), Times.Once);
        userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Test]
    public void AddUserAsync_AlreadyExists()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Dni = "43080644B",
            Password = "Password123",
            Role = Role.Admin.ToString()
        };

        var existingUser = new User
        {
            Id = "1",
            Dni = "43080644B",
            Password = "Password123",
            Role = Role.Admin
        };

        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userRequest.Dni)).ReturnsAsync(existingUser);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await userService.AddUserAsync(userRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"A user with the username '{userRequest.Dni}' already exists."));

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userRequest.Dni), Times.Once);
        userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task GetUserByUsernameAsync()
    {
        // Arrange
        string username = "User1"; 
        
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync(_user1);

        // Act
        var result = await userService.GetUserByUsernameAsync(username);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result); 
            ClassicAssert.AreEqual(_user1.Dni, result.Dni); 
            ClassicAssert.AreEqual("Admin", result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
    }
    
    [Test]
    public async Task GetUserByUsernameAsync_NotExists()
    {
        // Arrange
        string username = "NonExistentUser"; 
        
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(username)).ReturnsAsync((User)null);

        // Act
        Assert.ThrowsAsync<UserNotFoundException>(
            () => userService.GetUserByUsernameAsync(username)
        );

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(username), Times.Once);
    }

    
    [Test]
    public async Task UpdateUserAsync()
    {
        // Arrange
        var userId = "1";
        var userUpdateRequest = new UserUpdateRequest
        {
            Dni = "43080644B",
            Password = "UpdatedPassword123",
            Role = "User"
        };

        var existingUser = new User
        {
            Id = userId,
            Dni = "43080644B",
            Password = "Password123",
            Role = Role.Admin
        };

        var updatedUser = new User
        {
            Id = userId,
            Dni = "43080644B",
            Password = "UpdatedPassword123",
            Role = Role.User
        };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userUpdateRequest.Dni)).ReturnsAsync((User)null);
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await userService.UpdateUserAsync(userId, userUpdateRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(updatedUser.Dni, result.Dni);
            ClassicAssert.AreEqual("User", result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userUpdateRequest.Dni), Times.Once);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Test]
    public void UpdateUserAsync_WhenUsernameIsInvalid(){
        // Arrange
        var userId = "1";
        var userUpdateRequest = new UserUpdateRequest
        {
            Dni = "",
            Password = "UpdatedPassword123",
            Role = "User"
        };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidUsernameException>(async () =>
            await userService.UpdateUserAsync(userId, userUpdateRequest)
        );

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Never);
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userUpdateRequest.Dni), Times.Never);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Test]
    public async Task UpdateUserAsync_WhenFoundInCache()
    {
        // Arrange
        var userId = "1";
        var userUpdateRequest = new UserUpdateRequest
        {
            Dni = "43080644B",
            Password = "UpdatedPassword123",
            Role = "User"
        };

        var existingUser = new User
        {
            Id = userId,
            Dni = "43080644B",
            Password = "Password123",
            Role = Role.Admin
        };

        var updatedUser = new User
        {
            Id = userId,
            Dni = "43080644B",
            Password = "UpdatedPassword123",
            Role = Role.User
        };
        
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
           .ReturnsAsync((RedisValue)JsonSerializer.Serialize(existingUser));
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await userService.UpdateUserAsync(userId, userUpdateRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual("43080644B", result.Dni);
            ClassicAssert.AreEqual("User", result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Never);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Test]
    public async Task UpdateUserAsync_NotExist()
    {
        // Arrange
        var userId = "999"; 
        var userUpdateRequest = new UserUpdateRequest { Dni = "43080644B" };
    
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null); 
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await userService.UpdateUserAsync(userId, userUpdateRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"The user with id: {userId} was not found"));
    }
    
    [Test]
    public async Task UpdateUserAsync_UserNameTaken()
    {
        // Arrange
        var userId = "1"; 
        var userToUpdate = new User { Id = userId, Dni = "43080644B" }; 
        var userUpdateRequest = new UserUpdateRequest { Dni = "43080644B" };
        var existingUserSameName = new User { Id = "2", Dni = "43080644B" };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(existingUserSameName);
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await userService.UpdateUserAsync(userId, userUpdateRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"A user with the username '{userUpdateRequest.Dni}' already exists."));
    }


    
    [Test]
    public async Task DeleteUserAsync()
    {
        // Arrange
        var userId = "1"; 
        var userToUpdate = new User { Id = userId, Dni = "TestUser", IsDeleted = false };
        
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
    
        // Act
        await userService.DeleteUserAsync(userId, true); 
    
        // Assert
        ClassicAssert.IsTrue(userToUpdate.IsDeleted);
    
        // Verify
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
    
    [Test]
    public async Task DeleteUserAsync_SoftDeletion()
    {
        // Arrange
        var userId = "1";
        var userToUpdate = new User { Id = userId, Dni = "TestUser", IsDeleted = false };
        
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
        
        // Act
        await userService.DeleteUserAsync(userId, false); // Eliminación física
        
        // Assert
        ClassicAssert.IsFalse(userToUpdate.IsDeleted);
        
        // Verify
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
    }
    
    [Test]
    public async Task DeleteUserAsync_SoftDeletion_WhenFoundInCache()
    {
        // Arrange
        var userId = "1";
        var userToUpdate = new User { Id = userId, Dni = "TestUser", IsDeleted = false };
        
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
           .ReturnsAsync((RedisValue)JsonSerializer.Serialize(userToUpdate));
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        
        // Act
        await userService.DeleteUserAsync(userId, false); // Eliminación física
        
        // Assert
        userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Once);
    }
    
    [Test]
    public async Task DeleteUserAsync_NotExist()
    {
        // Arrange
        var userId = "999";

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null); 
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await userService.DeleteUserAsync(userId, true)
        );
        Assert.That(ex.Message, Is.EqualTo($"The user with id: {userId} was not found"));
    }
    
    [Test]
    public async Task DeleteUserAsync_PhysicalDeletion()
    {
        // Arrange
        var userId = "1";
        var userToUpdate = new User { Id = userId, Dni = "TestUser", IsDeleted = false }; 
        
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    
        // Act
        await userService.DeleteUserAsync(userId, false); // Eliminación física
    
        // Assert
        userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Once);
    }
}
