using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework.Legacy;
using Testcontainers.PostgreSql;
using VivesBankApi.Database;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;

namespace Tests.Rest.Clients.Repository
{
    public class ClientRepositoryTest
    {
        private PostgreSqlContainer _postgreSqlContainer;
        private BancoDbContext _dbContext;
        private ClientRepository _repository;

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
            
            _repository = new ClientRepository(
                _dbContext,
                NullLogger<ClientRepository>.Instance
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
        public async Task GetAllClientsPagedAsync_ShouldReturnPagedClients()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                await _repository.AddAsync(new Client
                {
                    UserId = $"DNI{i}",
                    FullName = $"Client {i}",
                    Adress = $"Address {i}",
                    Photo = "defaultId.png",
                    PhotoDni = "default.png",
                    IsDeleted = false
                });
            }

            // Act
            var result = await _repository.GetAllClientsPagedAsync(0, 5, "Client", false, "asc");

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(5, result.Count);
            ClassicAssert.AreEqual("Client 1", result[0].FullName);
        }

        [Test]
        public async Task GetAllClientsPagedAsync_ShouldApplyFiltersAndSorting()
        {
            // Arrange
            for (int i = 1; i <= 10; i++)
            {
                await _repository.AddAsync(new Client
                {
                    UserId = $"DNI{i}",
                    FullName = $"Client {i}",
                    Adress = $"Address {i}",
                    Photo = "defaultId.png",
                    PhotoDni = "default.png",
                    IsDeleted = false
                });
            }

            // Act
            var result = await _repository.GetAllClientsPagedAsync(0, 3, "Client", false, "desc");

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(3, result.Count);
            ClassicAssert.AreEqual("Client 9", result[0].FullName);
        }

        [Test]
        public async Task GetAllClientsPagedAsync_ShouldReturnEmpty_WhenNoClientsMatchFilter()
        {
            // Act
            var result = await _repository.GetAllClientsPagedAsync(0, 5, "NonexistentClient", false, "asc");

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(0, result.Count);
        }
    }
}
