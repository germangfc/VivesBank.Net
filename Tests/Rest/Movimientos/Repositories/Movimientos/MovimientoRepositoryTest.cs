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
    
    [SetUp]  // Se ejecuta UNA VEZ antes de todos los tests
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

    [TearDown]  // Se ejecuta UNA VEZ después de todos los tests
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
    public async Task GetMovimientoByIdAsync_NotFound()
    {
        // Act
        var result = await _repository.GetMovimientoByIdAsync(ObjectId.GenerateNewId().ToString());

        // Assert
        Assert.That(result, Is.Null);
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
    public async Task GetMovimientoByGuidAsync_NotFound()
    {
        // Act
        var result = await _repository.GetMovimientoByGuidAsync(Guid.NewGuid().ToString());

        // Assert
        Assert.That(result, Is.Null);
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
    public async Task UpdateMovimientoAsync_NotFound()
    {
        // Arrange
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };

        // Act
        var result = await _repository.UpdateMovimientoAsync(ObjectId.GenerateNewId().ToString(), movimiento);

        // Assert
        Assert.That(result, Is.Null);
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

    [Test]
    public async Task DeleteMovimientoAsync_NotFound()
    {
        // Act
       var result = await _repository.DeleteMovimientoAsync(ObjectId.GenerateNewId().ToString());

        // Assert
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task GetMovimientosDomiciliacionByClienteGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Domiciliacion = new Domiciliacion()
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetMovimientosDomiciliacionByClienteGuidAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
    
    [Test]
    public async Task GetMovimientosTransferenciaByClienteGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Transferencia = new Transferencia()
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetMovimientosTransferenciaByClienteGuidAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
    
    [Test]
    public async Task GetMovimientosPagoConTarjetaByClienteGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            PagoConTarjeta = new PagoConTarjeta()
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetMovimientosPagoConTarjetaByClienteGuidAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
    [Test]
    public async Task GetMovimientosReciboDeNominaByClienteGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            IngresoDeNomina = new IngresoDeNomina()
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetMovimientosReciboDeNominaByClienteGuidAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
    [Test]
    public async Task GetMovimientosTransferenciaRevocadaByClienteGuidAsync()
    {
        var movimiento = new Movimiento
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            IsDeleted = true
        };
        // Act
        await _repository.AddMovimientoAsync(movimiento);
        var movimientos = await _repository.GetMovimientosTransferenciaRevocadaByClienteGuidAsync(movimiento.ClienteGuid);

        // Assert
        Assert.That(movimientos, Is.Not.Empty);
        Assert.That(movimientos.First().Guid, Is.EqualTo(movimiento.Guid));
        Assert.That(movimientos.Count(), Is.EqualTo(1) );
    }
}