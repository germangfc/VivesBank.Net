using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Testcontainers.MongoDb;
using Testcontainers.PostgreSql;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;

namespace Tests.Rest.Movimientos.Repositories.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoRepository))]
public class MovimientoRepositoryTest
{
    private MongoDbContainer _mongoDbContainer;
    private IMovimientoRepository _repository;
    
    [OneTimeSetUp]  // Se ejecuta UNA VEZ antes de todos los tests
    public async Task GlobalSetup()
    {
        _mongoDbContainer = new MongoDbBuilder()
            .WithImage("mongo:4.4")
            .WithPortBinding(27017, true)
            .Build();

        await _mongoDbContainer.StartAsync();

        var mongoConfig = Options.Create(new MongoDatabaseConfig
        {
            ConnectionString = _mongoDbContainer.GetConnectionString(),
            DatabaseName = "testdb",
            MovimientosCollectionName = "movimientos"
        });

        _repository = new MovimientoRepository(mongoConfig, NullLogger<MovimientoRepository>.Instance);
    }

    [OneTimeTearDown]  // Se ejecuta UNA VEZ después de todos los tests
    public async Task GlobalTeardown()
    {
        if (_mongoDbContainer != null)
        {
            await _mongoDbContainer.StopAsync();  // Detiene el contenedor
            await _mongoDbContainer.DisposeAsync();  // Libera los recursos
        }
    }

    [Test]
    public async Task GetAllMovimientos()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetAllMovimientosAsync();

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
    
    [Test]
    public async Task GetMovimientoByIdAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddMovimientoAsync(movimiento);

        // Act
        var result = await _repository.GetMovimientoByIdAsync(movimiento.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(movimiento.Id));
    }
    
    [Test]
    public async Task GetMovimientoByGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddMovimientoAsync(movimiento);

        // Act
        var result = await _repository.GetMovimientoByGuidAsync(movimiento.Guid);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(movimiento.Id));
    }
    
    [Test]
    public async Task GetMovimientosByClientAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddMovimientoAsync(movimiento);

        // Act
        var result = await _repository.GetMovimientosByClientAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.First().Id, Is.EqualTo(movimiento.Id));
    }

    [Test]
    public async Task AddMovimientoAsync()
    {
        // Arrange
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };

        // Act
       var result = await _repository.AddMovimientoAsync(movimiento);
        

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(movimiento.Id));
    }

    [Test]
    public async Task UpdateMovimientoAsync()
    {
        // Arrange
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddMovimientoAsync(movimiento);

        movimiento.Guid = "Actualizado";

        // Act
        var result = await _repository.UpdateMovimientoAsync(movimiento.Id, movimiento);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("Actualizado"));
    }

    [Test]
    public async Task DeleteMovimientoAsync()
    {
        // Arrange
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddMovimientoAsync(movimiento);

        // Act
        await _repository.DeleteMovimientoAsync(movimiento.Id);

        // Assert
        var result = await _repository.GetMovimientoByIdAsync(movimiento.Id);
        Assert.That(result, Is.Null);
    }
}