using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Rest.Movimientos.Services.Domiciliaciones;

[TestFixture]
[TestOf(typeof(DomiciliacionService))]
public class DomiciliacionServiceTest
{
    private readonly IOptions<MongoDatabaseConfig> _mongoDatabaseSettings;
    private Mock<IDomiciliacionRepository> _repositoryMock;
    private Mock<ILogger<DomiciliacionService>> _loggerMock;
    private IOptions<ApiConfig> _apiConfig;
    private DomiciliacionService _domiciliacionService;
    private List<Domiciliacion> _expectedDomiciliacionList;

    [SetUp]
    public void Setup()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .Build();

        _apiConfig = Options.Create(configuration.GetSection("ApiBasicConfig").Get<ApiConfig>());
        _loggerMock = new Mock<ILogger<DomiciliacionService>>();
        _repositoryMock = new Mock<IDomiciliacionRepository>();
        _domiciliacionService = new DomiciliacionService(_repositoryMock.Object, _loggerMock.Object, _apiConfig);
        _expectedDomiciliacionList = new List<Domiciliacion>
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
    }
    [Test]
    public async Task FindAllDomiciliaciones()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllDomiciliacionesAsync())
            .ReturnsAsync(_expectedDomiciliacionList);

        // Act
        var result = await _domiciliacionService.FindAllDomiciliacionesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual(_expectedDomiciliacionList[0].Guid, result[0].Guid);
            ClassicAssert.AreEqual(_expectedDomiciliacionList[1].Guid, result[1].Guid);
            ClassicAssert.AreEqual(_expectedDomiciliacionList[0].ClienteGuid, result[0].ClienteGuid);
            ClassicAssert.AreEqual(_expectedDomiciliacionList[1].ClienteGuid, result[1].ClienteGuid);
        });

        _repositoryMock.Verify(repo => repo.GetAllDomiciliacionesAsync(), Times.Once);
    }
    
    [Test]
    public async Task FindDomiciliacionByIdAsyncOk()
    {
        
        // Arrange
        const string id = "1";
        var domiciliacion = _expectedDomiciliacionList.First();
        _repositoryMock.Setup(repo => repo.GetDomiciliacionByIdAsync(id))
            .ReturnsAsync(domiciliacion);
        
        // Act
        var result = await _domiciliacionService.FindDomiciliacionByIdAsync(id);
        
        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(domiciliacion.Guid, result.Guid);
        _repositoryMock.Verify(repo => repo.GetDomiciliacionByIdAsync(id), Times.Once);
    }
    
    [Test]
    public void FindDomiciliacionByIdAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        _repositoryMock.Setup(repo => repo.GetDomiciliacionByIdAsync(id))!.ReturnsAsync((Domiciliacion)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomiciliacionNotFoundException>(async () => 
            await _domiciliacionService.FindDomiciliacionByIdAsync(id));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Domiciliacion not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.GetDomiciliacionByIdAsync(id), Times.Once);
    }
    
    [Test]
    public async Task AddDomiciliacionAsyncOk()
    {
        // Arrange
        var newDomiciliacion = _expectedDomiciliacionList.First();
        _repositoryMock.Setup(repo => repo.AddDomiciliacionAsync(newDomiciliacion))
            .ReturnsAsync(newDomiciliacion);
        
        // Act
        var result = await _domiciliacionService.AddDomiciliacionAsync(newDomiciliacion);
        
        // Assert
        ClassicAssert.IsTrue(result.Contains(_apiConfig.Value.BaseEndpoint));
        
        _repositoryMock.Verify(repo => repo.AddDomiciliacionAsync(newDomiciliacion), Times.Once);
    }

    [Test]
    public async Task UpdateDomiciliacionAsyncOk()
    {
        // Arrange
        const string id = "1";
        var updatedDomiciliacion = _expectedDomiciliacionList.First();
        _repositoryMock.Setup(repo => repo.UpdateDomiciliacionAsync(id, updatedDomiciliacion)).
            ReturnsAsync(updatedDomiciliacion);
        
        // Act
        var result = await _domiciliacionService.UpdateDomiciliacionAsync(id, updatedDomiciliacion);
        
        // Assert
        ClassicAssert.IsTrue(result.Contains(_apiConfig.Value.BaseEndpoint));
        _repositoryMock.Verify(repo => repo.UpdateDomiciliacionAsync(id, updatedDomiciliacion), Times.Once);
    }

    [Test]
    public void UpdateDomiciliacionAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        var updatedDomiciliacion = _expectedDomiciliacionList.First();
        _repositoryMock.Setup(repo => repo.UpdateDomiciliacionAsync(id, updatedDomiciliacion))
            .ReturnsAsync((Domiciliacion)null);
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<DomiciliacionNotFoundException>(async () => 
            await _domiciliacionService.UpdateDomiciliacionAsync(id, updatedDomiciliacion));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Domiciliacion not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.UpdateDomiciliacionAsync(id, updatedDomiciliacion), Times.Once);
    }
    
    [Test]
    public async Task DeleteDomiciliacionAsyncOk()
    {
        // Arrange
        const string id = "1";
        var domiciliacion = _expectedDomiciliacionList.First();
        _repositoryMock.Setup(repo => repo.DeleteDomiciliacionAsync(id)).ReturnsAsync(domiciliacion);
        
        // Act
        var result = await _domiciliacionService.DeleteDomiciliacionAsync(id);
        
        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(domiciliacion, result);

        _repositoryMock.Verify(repo => repo.DeleteDomiciliacionAsync(id), Times.Once);
    }

    [Test]
    public void DeleteDomiciliacionAsyncIdNotFound()
    {
        // Arrange
        const string id = "invalidId";
        _repositoryMock.Setup(repo => repo.DeleteDomiciliacionAsync(id)).ReturnsAsync((Domiciliacion)null);
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<DomiciliacionNotFoundException>(async () => await _domiciliacionService.DeleteDomiciliacionAsync(id));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Domiciliacion not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.DeleteDomiciliacionAsync(id), Times.Once);
    }

}