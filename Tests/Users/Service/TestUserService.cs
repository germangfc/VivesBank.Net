using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Service;

[TestFixture]
public class TestUserService
{
    private Mock<IUserRepository> userRepositoryMock;
    private UserService userService;
    private User _user1;
    private User _user2;

    [SetUp]
    public void SetUp()
    {
        userRepositoryMock = new Mock<IUserRepository>();
        userService = new UserService(userRepositoryMock.Object);
        

        // Initialize common test data
        _user1 = new User
        {
            Id = "1",
            Username = "User1",
            Password = "Password123",
            Role = Role.Admin,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };

        _user2 = new User
        {
            Id = "2",
            Username = "User2",
            Password = "SecurePass456",
            Role = Role.User,
            CreatedAt = DateTime.Now.AddDays(-1),
            UpdatedAt = DateTime.Now,
            IsDeleted = false
        };
    }

    [Test]
    public async Task GetAll()
    {
        // Arrange
        var mockUsers = new List<User> { _user1, _user2 };
        userRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(mockUsers);

        // Act
        var result = await userService.GetAllUsersAsync();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);

            // Validate first user
            ClassicAssert.AreEqual(_user1.Username, result[0].Username);
            ClassicAssert.AreEqual(_user1.Password, result[0].Password);
            ClassicAssert.AreEqual(_user1.Role, result[0].Role);

            // Validate second user
            ClassicAssert.AreEqual(_user2.Username, result[1].Username);
            ClassicAssert.AreEqual(_user2.Password, result[1].Password);
            ClassicAssert.AreEqual(_user2.Role, result[1].Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }
    
    [Test]
    public async Task GetUserByIdAsync()
    {
        // Arrange
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(_user1.Id)).ReturnsAsync(_user1);

        // Act
        var result = await userService.GetUserByIdAsync(_user1.Id);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_user1.Username, result.Username);
            ClassicAssert.AreEqual(_user1.Password, result.Password);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(_user1.Id), Times.Once);
    }
    
    [Test]
    public async Task GetUserByIdAsync_NotExist()
    {
        // Arrange
        userRepositoryMock.Setup(repo => repo.GetByIdAsync("3")).ReturnsAsync((User)null);

        // Act
        var result = await userService.GetUserByIdAsync("3");

        // Assert
        ClassicAssert.IsNull(result);

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync("3"), Times.Once);
    }
    
    /*[Test]
    public async Task AddUserAsync_ShouldAddUser_WhenUserDoesNotExist()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Username = "User1",
            Password = "Password123",
            Role = Role.Admin
        };

        var newUser = UserMapper.ToUser(userRequest);

        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userRequest.Username)).ReturnsAsync((User)null);
        userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await userService.AddUserAsync(userRequest);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(result);
            Assert.AreEqual(newUser.Username, result.Username);
            Assert.AreEqual(newUser.Password, result.Password);
            Assert.AreEqual(newUser.Role, result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userRequest.Username), Times.Once);
        userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public void AddUserAsync_ShouldThrowException_WhenUserAlreadyExists()
    {
        // Arrange
        var userRequest = new CreateUserRequest
        {
            Username = "User1",
            Password = "Password123",
            Role = Role.Admin
        };

        var existingUser = new User
        {
            Id = "1",
            Username = "User1",
            Password = "Password123",
            Role = Role.Admin
        };

        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userRequest.Username)).ReturnsAsync(existingUser);

        // Act & Assert
        var ex = Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await userService.AddUserAsync(userRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"A user with the username '{userRequest.Username}' already exists."));

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userRequest.Username), Times.Once);
        userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
    }*/


    [Test]
    public async Task GetUserByUsernameAsync_Exists()
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
            ClassicAssert.AreEqual(_user1.Username, result.Username); 
            ClassicAssert.AreEqual(_user1.Password, result.Password); 
            ClassicAssert.AreEqual(_user1.Role, result.Role);
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
        var result = await userService.GetUserByUsernameAsync(username);

        // Assert
        ClassicAssert.IsNull(result);

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(username), Times.Once); // Verifica que se haya llamado al repositorio una vez
    }

    
    [Test]
    public async Task UpdateUserAsync()
    {
        // Arrange
        var userId = "1";
        var userUpdateRequest = new UserUpdateRequest
        {
            Username = "UpdatedUser",
            Password = "UpdatedPassword123",
            Role = "User"
        };

        var existingUser = new User
        {
            Id = userId,
            Username = "ExistingUser",
            Password = "ExistingPassword",
            Role = Role.Admin
        };

        var updatedUser = new User
        {
            Id = userId,
            Username = "UpdatedUser",
            Password = "UpdatedPassword123",
            Role = Role.User
        };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(userUpdateRequest.Username)).ReturnsAsync((User)null);
        userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

        // Act
        var result = await userService.UpdateUserAsync(userId, userUpdateRequest);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(updatedUser.Username, result.Username);
            ClassicAssert.AreEqual(updatedUser.Password, result.Password);
            ClassicAssert.AreEqual(updatedUser.Role, result.Role);
        });

        // Verify
        userRepositoryMock.Verify(repo => repo.GetByIdAsync(userId), Times.Once);
        userRepositoryMock.Verify(repo => repo.GetByUsernameAsync(userUpdateRequest.Username), Times.Once);
        userRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    
    [Test]
    public async Task UpdateUserAsync_NotExist()
    {
        // Arrange
        var userId = "aaaaaaa"; 
        var userUpdateRequest = new UserUpdateRequest { Username = "NewUsername" };
    
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null); 
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
            await userService.UpdateUserAsync(userId, userUpdateRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"The user with id: {userId} was not found"));
    }
    
    [Test]
    public async Task UpdateUserAsync_UserNameIsTakenByAnotherUser()
    {
        // Arrange
        var userId = "1"; // ID del usuario
        var userToUpdate = new User { Id = userId, Username = "User1" }; 
        var userUpdateRequest = new UserUpdateRequest { Username = "NewUsername" };

        var existingUserSameName = new User { Id = "2", Username = "NewUsername" };

        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync(existingUserSameName);
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await userService.UpdateUserAsync(userId, userUpdateRequest)
        );
        Assert.That(ex.Message, Is.EqualTo($"A user with the username '{userUpdateRequest.Username}' already exists."));
    }


    
    [Test]
    public async Task DeleteUserAsync()
    {
        // Arrange
        var userId = "1"; 
        var userToUpdate = new User { Id = userId, Username = "TestUser", IsDeleted = false };
        
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
    public async Task DeleteUserAsync_NotExist()
    {
        // Arrange
        var userId = "aaaaaaaaa";
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null); 
        
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
        var userToUpdate = new User { Id = userId, Username = "TestUser", IsDeleted = false }; 
    
        userRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(userToUpdate);
        userRepositoryMock.Setup(repo => repo.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    
        // Act
        await userService.DeleteUserAsync(userId, false); // Eliminación física
    
        // Assert
        userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Once);
    }

}
