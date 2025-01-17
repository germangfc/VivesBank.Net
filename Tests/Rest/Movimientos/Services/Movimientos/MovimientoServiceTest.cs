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
using VivesBankApi.Rest.Users.Models;
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
    public async Task FindMovimientoByIdAsyncOk()
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
        ClassicAssert.AreEqual(ex.Message, $"No se encontro el movimiento con el ID/Guid {id}");
        
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
    public async Task UpdateMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        var updatedMovimiento = _expectedMovimientoList.First();
        _repositoryMock.Setup(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento))!
            .ReturnsAsync((Movimiento)null);
        
        // Act & Assert
        var result = await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento);
        
        // Assert
        ClassicAssert.IsNull(result);
        
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
    public async Task DeleteMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        _repositoryMock.Setup(repo => repo.DeleteMovimientoAsync(id))!.ReturnsAsync((Movimiento)null!);
        
        // Act & Assert
        var result = await _movimientoService.DeleteMovimientoAsync(id);
        
        // Assert
        ClassicAssert.IsNull(result);
        
        _repositoryMock.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Once);
    }

    [Test]
    public async Task FindMovimientoByGuidAsyncOk()
    {
        // Arrange
        const string guid = "G1";
        var expectedMovimiento = _expectedMovimientoList.First();
    
        _repositoryMock.Setup(repo => repo.GetMovimientoByGuidAsync(guid))
            .ReturnsAsync(expectedMovimiento);  // Simulamos que el repositorio devuelve el movimiento esperado

        // Act
        var result = await _movimientoService.FindMovimientoByGuidAsync(guid);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(expectedMovimiento.Guid, result.Guid);
        
        _repositoryMock.Verify(repo => repo.GetMovimientoByGuidAsync(guid), Times.Once);
    }

    [Test]
    public async Task FindMovimientoByGuidAsyncNotFound()
    {
        // Arrange
        const string guid = "xxx"; 
        _repositoryMock.Setup(repo => repo.GetMovimientoByGuidAsync(guid))!
            .ReturnsAsync((Movimiento)null!);  
    
        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () => 
            await _movimientoService.FindMovimientoByGuidAsync(guid));
        
        // Assert
        ClassicAssert.AreEqual(ex.Message, $"No se encontro el movimiento con el ID/Guid {guid}");
        
        _repositoryMock.Verify(repo => repo.GetMovimientoByGuidAsync(guid), Times.Once);
    }

    [Test]
    public async Task FindAllMovimientosByClientAsyncOk()
    {
        // Arrange
        const string clienteId = "1";  
        _repositoryMock.Setup(repo => repo.GetMovimientosByClientAsync(clienteId))
            .ReturnsAsync(_expectedMovimientoList);  

        // Act
        var result = await _movimientoService.FindAllMovimientosByClientAsync(clienteId);

        // Assert
        ClassicAssert.AreEqual(_expectedMovimientoList, result);  
        
        _repositoryMock.Verify(repo => repo.GetMovimientosByClientAsync(clienteId), Times.Once);
    }
    
    [Test]
    public async Task FindAllMovimientosByClientAsyncEmptyList()
    {
        // Arrange
        const string clienteId = "10";  
        var expectedEmptyList = new List<Movimiento>();  

        _repositoryMock.Setup(repo => repo.GetMovimientosByClientAsync(clienteId))
            .ReturnsAsync(expectedEmptyList);

        // Act
        var result = await _movimientoService.FindAllMovimientosByClientAsync(clienteId);

        // Assert
        ClassicAssert.AreEqual(expectedEmptyList, result);
        
        _repositoryMock.Verify(repo => repo.GetMovimientosByClientAsync(clienteId), Times.Once);
        
    }


    
    
    
    [Test]
    public void AddDomiciliacionAsyncNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _movimientoService.AddDomiciliacionAsync(new User(), new Domiciliacion()));
    }
    
    [Test]
    public async Task AddIngresoDeNominaAsyncNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _movimientoService.AddIngresoDeNominaAsync(new User(), new IngresoDeNomina()));
    }
    
    [Test]
    public async Task AddPagoConTarjetaAsyncNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _movimientoService.AddPagoConTarjetaAsync(new User(), new PagoConTarjeta()));
    }

    [Test]
    public async Task AddTransferenciaAsyncNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _movimientoService.AddTransferenciaAsync(new User(), new Transferencia()));
    }
    [Test]
    public async Task RevocarTransferenciaNotImplementedException()
    {
        // Act & Assert
        Assert.ThrowsAsync<NotImplementedException>(async () => 
            await _movimientoService.RevocarTransferencia(new User(), "some-guid"));
    }

}