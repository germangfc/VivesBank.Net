using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Legacy;
using Testcontainers.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace Tests.Rest.Users.Repository;

public class UserRepositoryTest
{
    private PostgreSqlContainer _postgreSqlContainer;
    private BancoDbContext _dbContext;
    private UserRepository _repository;

    [OneTimeSetUp]
    public async Task Setup()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .WithPortBinding(5432, true)
            .Build();

        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<BancoDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _dbContext = new BancoDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        _repository = new UserRepository(
            _dbContext,
            NullLogger<UserRepository>.Instance
        );

        await _repository.DeleteAllAsync();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _repository.DeleteAllAsync();
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        if (_postgreSqlContainer != null)
        {
            await _postgreSqlContainer.StopAsync();
            await _postgreSqlContainer.DisposeAsync();
        }
    }

    [Test]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Dni = "testuser",
            Password = "testpassword",
            Role = Role.User,
            IsDeleted = false
        };
        await _repository.AddAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("testuser", result.Dni);
    }

    [Test]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _repository.GetByUsernameAsync("nonexistentuser");

        // Assert
        ClassicAssert.Null(result);
    }

    [Test]
    public async Task GetAllUsersPagedAsync_ShouldReturnPagedUsers()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddAsync(new User
            {
                Dni = $"user{i}",
                Role = Role.User,
                Password = "testpassword",
                IsDeleted = false
            });
        }

        // Act
        var result = await _repository.GetAllUsersPagedAsync(0, 5, "User", false, "asc");

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(5, result.Count);
        ClassicAssert.AreEqual("user1", result[0].Dni);
    }

    [Test]
    public async Task GetAllUsersPagedAsync_ShouldApplyFiltersAndSorting()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddAsync(new User
            {
                Dni = $"user{i}",
                Role = i % 2 == 0 ? Role.Admin : Role.User,
                Password = "testpassword",
                IsDeleted = false
            });
        }

        // Act
        var result = await _repository.GetAllUsersPagedAsync(0, 3, "Admin", false, "desc");

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(3, result.Count);
        ClassicAssert.AreEqual("user8", result[0].Dni);
    }
}
