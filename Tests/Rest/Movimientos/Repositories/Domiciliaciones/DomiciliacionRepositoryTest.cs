using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using Moq;
using NUnit.Framework.Legacy;
using Testcontainers.MongoDb;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Rest.Movimientos.Repositories.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionRepository))]
public class DomiciliacionRepositoryTest
{
    private MongoDbContainer _mongoDbContainer;
    private IDomiciliacionRepository _repository;

    [SetUp]
    public async Task Setup()
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
            DomiciliacionCollectionName = "domiciliaciones"
        });

        _repository = new DomiciliacionRepository(mongoConfig, NullLogger<DomiciliacionRepository>.Instance);
    }

    [TearDown] // Se ejecuta UNA VEZ después de todos los tests
    public async Task TearDown()
    {
            await _mongoDbContainer.StopAsync(); // Detiene el contenedor
            await _mongoDbContainer.DisposeAsync(); // Libera los recursos
    }

    [Test]
    public async Task GetAllDomiciliaciones()
    {
        var domiciliacion = new Domiciliacion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var domiciliaciones = await _repository.GetAllDomiciliacionesAsync();

        Assert.That(domiciliaciones, Is.Not.Empty);
        Assert.That(domiciliaciones.First().Guid, Is.EqualTo(domiciliacion.Guid));
        Assert.That(domiciliaciones.Count(), Is.EqualTo(1));
    }
    [Test]
    public async Task GetAllDomiciliacionesActivasAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Activa = true,
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var domiciliaciones = await _repository.GetAllDomiciliacionesActivasAsync();

        Assert.That(domiciliaciones, Is.Not.Empty);
        Assert.That(domiciliaciones.First().Guid, Is.EqualTo(domiciliacion.Guid));
        Assert.That(domiciliaciones.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetDomiciliacionByIdAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var domiciliacionById = await _repository.GetDomiciliacionByIdAsync(domiciliacion.Id);

        Assert.That(domiciliacionById, Is.Not.Null);
        Assert.That(domiciliacionById.Guid, Is.EqualTo(domiciliacion.Guid));

    }
    [Test]
    public async Task GetDomiciliacionByIdAsync_NotFound()
    {
        var result = await _repository.GetDomiciliacionByIdAsync(ObjectId.GenerateNewId().ToString());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task AddDomiciliacionAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        var result = await _repository.AddDomiciliacionAsync(domiciliacion);

        Assert.That(result.Id, Is.Not.Null);
        Assert.That(result.Guid, Is.Not.Null);
        Assert.That(result.ClienteGuid, Is.Not.Null);
    }

    [Test]
    public async Task UpdateDomiciliacionAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        domiciliacion.Guid = "Nueva descripcion";
        var result = await _repository.UpdateDomiciliacionAsync(domiciliacion.Id, domiciliacion);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Guid, Is.EqualTo("Nueva descripcion"));
    }
    
    [Test]
    public async Task UpdateDomiciliacionesAsync_NotFund()
    {
        var result = await _repository.UpdateDomiciliacionAsync(ObjectId.GenerateNewId().ToString(), new Domiciliacion());
        Assert.That(result, Is.Null);
    }
    
    [Test]
    public async Task DeleteDomiciliacionAsync_NotFound()
    {
        var result = await _repository.DeleteDomiciliacionAsync(ObjectId.GenerateNewId().ToString());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteDomiciliacionAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var result = await _repository.DeleteDomiciliacionAsync(domiciliacion.Id);

        Assert.That(result, Is.Not.Null);
        
    }
    
    [Test]
    public async Task GetDomiciliacionesActivasByClienteGiudAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Activa = true,
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var domiciliaciones = await _repository.GetDomiciliacionesActivasByClienteGiudAsync(domiciliacion.ClienteGuid);

        Assert.That(domiciliaciones, Is.Not.Empty);
        Assert.That(domiciliaciones.First().Guid, Is.EqualTo(domiciliacion.Guid));
        Assert.That(domiciliaciones.Count(), Is.EqualTo(1));
    }
    [Test]
    public async Task GetDomiciliacionByClientGuidAsync()
    {
        var domiciliacion = new Domiciliacion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Guid = Guid.NewGuid().ToString(),
            ClienteGuid = Guid.NewGuid().ToString(),
            Activa = false,
        };
        await _repository.AddDomiciliacionAsync(domiciliacion);
        var domiciliaciones = await _repository.GetDomiciliacionByClientGuidAsync(domiciliacion.ClienteGuid);

        Assert.That(domiciliaciones, Is.Not.Empty);
        Assert.That(domiciliaciones.First().Guid, Is.EqualTo(domiciliacion.Guid));
        Assert.That(domiciliaciones.Count(), Is.EqualTo(1));
    }
}