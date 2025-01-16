using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Rest.Movimientos.Repositories.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionRepository))]
public class DomiciliacionRepositoryTest
{
    private MongoDbRunner _mongoDbRunner;
    private IMongoDatabase _database;
    private IMongoCollection<Domiciliacion> _collection;
    private Mock<ILogger<DomiciliacionRepository>> _mockLogger;
    private DomiciliacionRepository _repository;
    private Mock<IOptions<MongoDatabaseConfig>> _mockMongoDatabaseSettings;
    private readonly string _dataBaseName = "TestDatabase";

    [SetUp]
    public void SetUp()
    {
        // Inicializar MongoDB en memoria
        _mongoDbRunner = MongoDbRunner.Start();
        
        // Crear configuración de base de datos en memoria
        _mockMongoDatabaseSettings = new Mock<IOptions<MongoDatabaseConfig>>();
        _mockMongoDatabaseSettings.Setup(m => m.Value).Returns(new MongoDatabaseConfig
        {
            ConnectionString = _mongoDbRunner.ConnectionString,
            //DatabaseName = _mongoDbRunner.DatabaseName,
            DatabaseName = _dataBaseName,
            DomiciliacionCollectionName = "Domiciliaciones"
        });

        // Conectar a la base de datos en memoria
        var client = new MongoClient(_mongoDbRunner.ConnectionString);
        _database = client.GetDatabase(_dataBaseName);
        _collection = _database.GetCollection<Domiciliacion>("Domiciliaciones");

        // Mock de Logger
        _mockLogger = new Mock<ILogger<DomiciliacionRepository>>();

        // Crear el repositorio
        _repository = new DomiciliacionRepository(
            _mockMongoDatabaseSettings.Object,
            _mockLogger.Object
        );
    }

    [TearDown]
    public void TearDown()
    {
        // Detener el servidor en memoria
        _mongoDbRunner.Dispose();
    }

    [Test]
    public async Task GetAllDomiciliacionesAsync_ReturnsListOfDomiciliaciones()
    {
        // Arrange
        var expectedList = new List<Domiciliacion>
        {
            new Domiciliacion { 
                Guid = GuuidGenerator.GenerateHash(),
                ClienteGuid = "Cliente1",
                IbanOrigen = "ES12345678901234567890",
                IbanDestino = "ES98765432109876543210",
                Cantidad = 100,
                NombreAcreedor = "Acreedor1",
                FechaInicio = new DateTime(2021, 1, 1),
                Periodicidad = Periodicidad.SEMANAL,
                Activa = true,
                UltimaEjecucion = new DateTime(2024, 1, 1)
            },
            new Domiciliacion {  
                Guid = GuuidGenerator.GenerateHash(),
                ClienteGuid = "Cliente1",
                IbanOrigen = "ES12345678901234567890",
                IbanDestino = "ES98765432109876543210",
                Cantidad = 200,
                NombreAcreedor = "Acreedor2",
                FechaInicio = new DateTime(2024, 10, 23),
                Periodicidad = Periodicidad.ANUAL,
                Activa = true,
                UltimaEjecucion = new DateTime(2025, 1, 2)
            }
        };

        await _collection.InsertManyAsync(expectedList);

        // Act
        var result = await _repository.GetAllDomiciliacionesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.IsNotEmpty(result);
            ClassicAssert.AreEqual(expectedList.Count, result.Count);
            ClassicAssert.AreEqual(expectedList[0].Id, result[0].Id);
            ClassicAssert.AreEqual(expectedList[1].Id, result[1].Id);
            ClassicAssert.AreEqual(expectedList[0].Guid, result[0].Guid);
            ClassicAssert.AreEqual(expectedList[1].Guid, result[1].Guid);
            ClassicAssert.AreEqual(expectedList[0].ClienteGuid, result[0].ClienteGuid);
            ClassicAssert.AreEqual(expectedList[1].ClienteGuid, result[1].ClienteGuid);
            ClassicAssert.AreEqual(expectedList[0].IbanOrigen, result[0].IbanOrigen);
            ClassicAssert.AreEqual(expectedList[1].IbanOrigen, result[1].IbanOrigen);
        });

    }
}