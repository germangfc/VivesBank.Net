using NUnit.Framework;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using NUnit.Framework.Legacy;

namespace Tests.Rest.Users.Mapper;

public class UserMapperTests
{
    private readonly User _user1;
    private readonly User _user2;

    public UserMapperTests()
    {
        _user1 = new User
        {
            Id = "1",
            Dni = "user1",
            Password = "hashedPassword1",
            Role = Role.User,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };

        _user2 = new User
        {
            Id = "2",
            Dni = "user2",
            Password = "hashedPassword2",
            Role = Role.Admin,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
            IsDeleted = false
        };
    }

    [Test]
    public void ToUserResponse()
    {
        // Act
        var result = UserMapper.ToUserResponse(_user1);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_user1.Id, result.Id);
            ClassicAssert.AreEqual(_user1.Dni, result.Dni);
            ClassicAssert.AreEqual(_user1.Role.ToString(), result.Role);
            ClassicAssert.AreEqual(_user1.CreatedAt.ToLocalTime(), result.CreatedAt);
            ClassicAssert.AreEqual(_user1.UpdatedAt.ToLocalTime(), result.UpdatedAt);
            ClassicAssert.AreEqual(_user1.IsDeleted, result.IsDeleted);
        });
    }

    [Test]
    public void UpdateUserFromInput()
    {
        // Arrange
        var request = new UserUpdateRequest
        {
            Dni = "updatedUsername",
            Password = "updatedPassword",
            Role = "Admin"
        };

        // Act
        var updatedUser = UserMapper.UpdateUserFromInput(request, _user1);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(updatedUser);
            ClassicAssert.AreEqual("updatedUsername", updatedUser.Dni);
            ClassicAssert.IsNotNull(updatedUser.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify(request.Password, updatedUser.Password), "Password hash does not match");
            ClassicAssert.AreEqual(Role.Admin, updatedUser.Role);
        });
    }

    [Test]
    public void UpdateUserFromInput_SuperAdmin()
    {
        // Arrange
        var request = new UserUpdateRequest
        {
            Dni = "updatedUsername",
            Password = "updatedPassword",
            Role = "SuperAdmin"
        };

        // Act
        var updatedUser = UserMapper.UpdateUserFromInput(request, _user1);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(updatedUser);
            ClassicAssert.AreEqual("updatedUsername", updatedUser.Dni);
            ClassicAssert.IsNotNull(updatedUser.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify(request.Password, updatedUser.Password), "Password hash does not match");
            ClassicAssert.AreEqual(Role.SuperAdmin, updatedUser.Role);
        });
    }

    [Test]
    public void UpdateUserFromInput_InvalidRole()
    {
        // Arrange
        var request = new UserUpdateRequest
        {
            Role = "InvalidRole"
        };

        // Act & Assert
        Assert.Throws<InvalidRoleException>(() => UserMapper.UpdateUserFromInput(request, _user1));
    }

    [Test]
    public void ToUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Dni = "newuser",
            Password = "securePassword",
            Role = "SuperAdmin"
        };

        // Act
        var newUser = UserMapper.ToUser(request);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(newUser);
            ClassicAssert.AreEqual(request.Dni, newUser.Dni);
            ClassicAssert.IsNotNull(newUser.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify(request.Password, newUser.Password), "Password hash does not match");
            ClassicAssert.AreEqual(Role.SuperAdmin, newUser.Role);
        });
    }

    [Test]
    public void ToUser_SuperAdmin()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Dni = "newuser",
            Password = "securePassword",
            Role = "SuperAdmin"
        };

        // Act
        var newUser = UserMapper.ToUser(request);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(newUser);
            ClassicAssert.AreEqual(request.Dni, newUser.Dni);
            ClassicAssert.IsNotNull(newUser.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify(request.Password, newUser.Password), "Password hash does not match");
            ClassicAssert.AreEqual(Role.SuperAdmin, newUser.Role);
        });
    }

    [Test]
    public void ToUser_InvalidRole()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Dni = "newuser",
            Password = "securePassword",
            Role = "InvalidRole"
        };

        // Act & Assert
        Assert.Throws<InvalidRoleException>(() => UserMapper.ToUser(request));
    }

    [Test]
    public void ToUser_ResponseMultiple()
    {
        // Arrange
        var users = new List<User> { _user1, _user2 };

        // Act
        var result = users.ConvertAll(UserMapper.ToUserResponse);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);

            ClassicAssert.AreEqual(_user1.Dni, result[0].Dni);
            ClassicAssert.AreEqual(_user1.Role.ToString(), result[0].Role);

            ClassicAssert.AreEqual(_user2.Dni, result[1].Dni);
            ClassicAssert.AreEqual(_user2.Role.ToString(), result[1].Role);
        });
    }
    
    [Test]
    public void ToUser_UserRole()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Dni = "newuser",
            Password = "securePassword",
            Role = "user"
        };

        // Act
        var newUser = UserMapper.ToUser(request);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(newUser);
            ClassicAssert.AreEqual(request.Dni, newUser.Dni);
            ClassicAssert.IsNotNull(newUser.Password);
            ClassicAssert.IsTrue(BCrypt.Net.BCrypt.Verify(request.Password, newUser.Password), "Password hash does not match");
            ClassicAssert.AreEqual(Role.User, newUser.Role);
        });
    }
}