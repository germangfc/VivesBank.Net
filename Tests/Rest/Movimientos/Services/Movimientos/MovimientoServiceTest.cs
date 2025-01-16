using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Rest.Movimientos.Services.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoService))]
public class MovimientoServiceTest
{
    private readonly IOptions<MongoDatabaseConfig> _mongoDatabaseSettings;
    private Mock<IMovimientoRepository> _repositoryMock;
    private Mock<ILogger<MovimientoService>> _loggerMock;
    private IOptions<ApiConfig> _apiConfig;
    private MovimientoService _movimientoService;
    private List<Movimiento> _expectedMovimientoList;

    [SetUp]
    public void Setup()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .Build();

        _apiConfig = Options.Create(configuration.GetSection("ApiBasicConfig").Get<ApiConfig>());
        _loggerMock = new Mock<ILogger<MovimientoService>>();
        _repositoryMock = new Mock<IMovimientoRepository>();
        _movimientoService = new MovimientoService(_repositoryMock.Object, _loggerMock.Object, _apiConfig);
        _expectedMovimientoList = new List<Movimiento>
        {
            new Movimiento { 
                Guid = GuuidGenerator.GenerateHash(),
                ClienteGuid = "Cliente1"
            },
            new Movimiento {  
                Guid = GuuidGenerator.GenerateHash(),
                ClienteGuid = "Cliente2"
            }
        };
    }
    [Test]
    public async Task FindAllMovimientosOk()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllMovimientosAsync())
            .ReturnsAsync(_expectedMovimientoList);

        // Act
        var result = await _movimientoService.FindAllMovimientosAsync();

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(2, result.Count);
            ClassicAssert.AreEqual(_expectedMovimientoList[0].Guid, result[0].Guid);
            ClassicAssert.AreEqual(_expectedMovimientoList[1].Guid, result[1].Guid);
            ClassicAssert.AreEqual(_expectedMovimientoList[0].ClienteGuid, result[0].ClienteGuid);
            ClassicAssert.AreEqual(_expectedMovimientoList[1].ClienteGuid, result[1].ClienteGuid);
        });

        _repositoryMock.Verify(repo => repo.GetAllMovimientosAsync(), Times.Once);
    }
    
    [Test]
    public async Task FindDomiciliacionByIdAsyncOk()
    {
        
        // Arrange
        const string id = "1";
        var movimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(id))
            .ReturnsAsync(movimiento);
        
        // Act
        var result = await _movimientoService.FindMovimientoByIdAsync(id);
        
        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(movimiento.Guid, result.Guid);
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
    }
    
    [Test]
    public void FindMovimientoByIdAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(id))!.ReturnsAsync((Movimiento)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () => 
            await _movimientoService.FindMovimientoByIdAsync(id));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Movimiento not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
    }
    
    [Test]
    public async Task AddMovimientoAsyncOk()
    {
        // Arrange
        var newMovimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.AddMovimientoAsync(newMovimiento))
            .ReturnsAsync(newMovimiento);
        
        // Act
        var result = await _movimientoService.AddMovimientoAsync(newMovimiento);
        
        // Assert
        ClassicAssert.AreEqual(newMovimiento, result);
        
        _repositoryMock.Verify(repo => repo.AddMovimientoAsync(newMovimiento), Times.Once);
    }

    [Test]
    public async Task UpdateMovimientoAsyncOk()
    {
        // Arrange
        const string id = "1";
        var updatedMovimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento)).
            ReturnsAsync(updatedMovimiento);
        
        // Act
        var result = await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento);
        
        // Assert
        ClassicAssert.AreEqual(updatedMovimiento, result);
        _repositoryMock.Verify(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento), Times.Once);
    }

    [Test]
    public void UpdateMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        var updatedMovimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento))
            .ReturnsAsync((Movimiento)null);
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () => 
            await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Movimiento not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento), Times.Once);
    }
    
    [Test]
    public async Task DeleteMovimientoAsyncOk()
    {
        // Arrange
        const string id = "1";
        var movimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.DeleteMovimientoAsync(id)).ReturnsAsync(movimiento);
        
        // Act
        var result = await _movimientoService.DeleteMovimientoAsync(id);
        
        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(movimiento, result);
        
        _repositoryMock.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Once);
    }

    [Test]
    public void DeleteMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "invalidId";
        _repositoryMock.Setup(repo => repo.DeleteMovimientoAsync(id)).ReturnsAsync((Movimiento)null);
        
        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () => await _movimientoService.DeleteMovimientoAsync(id));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"Movimiento not found with ID {id}");
        
        _repositoryMock.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Once);
    }

}