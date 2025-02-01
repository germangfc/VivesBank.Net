/*using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.utils.GuuidGenerator;

namespace Tests.Rest.Movimientos.Services.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoService))]
public class MovimientoServiceTest
{
    private readonly IOptions<MongoDatabaseConfig> _mongoDatabaseSettings;
    private Mock<IMovimientoRepository> _repositoryMock;
    private Mock<IDomiciliacionRepository> _domiciliacionRepository;
    private Mock<IUserService> _userServiceMock;
    private Mock<IClientService> _clientServiceMock;
    private Mock<IAccountsService> _accountsServiceMock;
    private Mock<ICreditCardService> _creditCardServiceMock;
    private Mock<ILogger<MovimientoService>> _loggerMock;
    private IOptions<ApiConfig> _apiConfig;
    private MovimientoService _movimientoService;
    private List<Movimiento> _expectedMovimientoList;
    private Mock<IConnectionMultiplexer> _connection;
    private Mock<IDatabase> _cache;

    private Movimiento _movimiento1 = new Movimiento
    {
        Id = "1",
        Guid = "some-guid",
        ClienteGuid = "client-guid",
        Domiciliacion = new Domiciliacion{},
        IngresoDeNomina = new IngresoDeNomina{},
        PagoConTarjeta = new PagoConTarjeta{},
        Transferencia = new Transferencia{},
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };
    
    private Movimiento _movimiento2 = new Movimiento
    {
        Id = "2",
        Guid = "some-guid2",
        ClienteGuid = "client-guid2",
        Domiciliacion = new Domiciliacion{},
        IngresoDeNomina = new IngresoDeNomina{},
        PagoConTarjeta = new PagoConTarjeta{},
        Transferencia = new Transferencia{},
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

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
        _cache = new Mock<IDatabase>();
        _connection = new Mock<IConnectionMultiplexer>();
        _connection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<string>())).Returns(_cache.Object);

        _domiciliacionRepository = new Mock<IDomiciliacionRepository>();
        _userServiceMock = new Mock<IUserService>();
        _clientServiceMock = new Mock<IClientService>();
        _accountsServiceMock = new Mock<IAccountsService>();
        _creditCardServiceMock = new Mock<ICreditCardService>();

        _movimientoService = new MovimientoService(
            _repositoryMock.Object,
            _domiciliacionRepository.Object,
            _userServiceMock.Object,
            _clientServiceMock.Object,
            _accountsServiceMock.Object,
            _creditCardServiceMock.Object,
            _loggerMock.Object,
            _apiConfig,
            _connection.Object
        );
        _expectedMovimientoList = new List<Movimiento>
        {
             _movimiento1, _movimiento2
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

            var movimiento1 = result[0];
            ClassicAssert.AreEqual(_movimiento1.Id, movimiento1.Id);
            ClassicAssert.AreEqual(_movimiento1.Guid, movimiento1.Guid);
            ClassicAssert.AreEqual(_movimiento1.ClienteGuid, movimiento1.ClienteGuid);

            var movimiento2 = result[1];
            ClassicAssert.AreEqual(_movimiento2.Id, movimiento2.Id);
            ClassicAssert.AreEqual(_movimiento2.Guid, movimiento2.Guid);
            ClassicAssert.AreEqual(_movimiento2.ClienteGuid, movimiento2.ClienteGuid);
        });

        _repositoryMock.Verify(repo => repo.GetAllMovimientosAsync(), Times.Once);
    }
    
    [Test]
    public async Task GetMovimientoByIdAsync_WhenInCache()
    {
        // Arrange
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonConvert.SerializeObject(_movimiento1));

        // Act
        var result = await _movimientoService.FindMovimientoByIdAsync(_movimiento1.Id);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_movimiento1.Id, result.Id);
            ClassicAssert.AreEqual(_movimiento1.Guid, result.Guid);
            ClassicAssert.AreEqual(_movimiento1.ClienteGuid, result.ClienteGuid);
        });

        // Verify
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(_movimiento1.Id), Times.Never);
    }
    
    [Test]
    public async Task GetMovimientoByGuidAsync_WhenInCache()
    {
        // Arrange
        _cache.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonConvert.SerializeObject(_movimiento1));

        // Act
        var result = await _movimientoService.FindMovimientoByGuidAsync(_movimiento1.Guid);

        // Assert
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(_movimiento1.Id, result.Id);
            ClassicAssert.AreEqual(_movimiento1.Guid, result.Guid);
            ClassicAssert.AreEqual(_movimiento1.ClienteGuid, result.ClienteGuid);
        });

        // Verify
        _repositoryMock.Verify(repo => repo.GetMovimientoByGuidAsync(_movimiento1.Guid), Times.Never);
    }
    
    [Test]
    public async Task GetMovimientoByIdAsync_CacheNotFound()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(It.IsAny<string>())).ReturnsAsync((Movimiento)null);

        // Act & Assert
        Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
        {
            await _movimientoService.FindMovimientoByIdAsync("999");
        });
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
        var updatedMovimiento = _movimiento2;
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(_movimiento1.Id))
            .ReturnsAsync(updatedMovimiento); 
        _repositoryMock.Setup(repo => repo.UpdateMovimientoAsync(_movimiento1.Id, updatedMovimiento))
            .ReturnsAsync(updatedMovimiento);

        // Act
        var result = await _movimientoService.UpdateMovimientoAsync(_movimiento1.Id, updatedMovimiento);

        // Assert
        ClassicAssert.AreEqual(updatedMovimiento, result);
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(_movimiento1.Id), Times.Once); 
        _repositoryMock.Verify(repo => repo.UpdateMovimientoAsync(_movimiento1.Id, updatedMovimiento), Times.Once);
    }

    [Test]
    public async Task UpdateMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        var updatedMovimiento = _movimiento1;
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(id))
            .ReturnsAsync((Movimiento)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
            await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento));

        // Assert
        ClassicAssert.AreEqual(ex.Message, $"No se encontro el movimiento con el ID/Guid {id}");
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento), Times.Never);
    }
    
    [Test]
    public async Task DeleteMovimientoAsyncOk()
    {
        // Arrange
        var movimiento = _movimiento1;
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(_movimiento1.Id))
            .ReturnsAsync(movimiento);
        _repositoryMock.Setup(repo => repo.DeleteMovimientoAsync(_movimiento1.Id))
            .ReturnsAsync(movimiento);

        // Act
        var result = await _movimientoService.DeleteMovimientoAsync(_movimiento1.Id);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(movimiento, result);
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(_movimiento1.Id), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteMovimientoAsync(_movimiento1.Id), Times.Once);
    }

    [Test]
    public async Task DeleteMovimientoAsyncIdNotFound()
    {
        // Arrange
        const string id = "xxx";
        _repositoryMock.Setup(repo => repo.GetMovimientoByIdAsync(id))
            .ReturnsAsync((Movimiento)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
            await _movimientoService.DeleteMovimientoAsync(id));

        // Assert
        ClassicAssert.AreEqual(ex.Message, $"No se encontro el movimiento con el ID/Guid {id}");
        _repositoryMock.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
        _repositoryMock.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Never);
    }

    [Test]
    public async Task FindMovimientoByGuidAsyncOk()
    {
        // Arrange
        var expectedMovimiento = _movimiento1;
    
        _repositoryMock.Setup(repo => repo.GetMovimientoByGuidAsync(_movimiento1.Guid))
            .ReturnsAsync(expectedMovimiento);

        // Act
        var result = await _movimientoService.FindMovimientoByGuidAsync(_movimiento1.Guid);

        // Assert
        ClassicAssert.IsNotNull(result);
        ClassicAssert.AreEqual(expectedMovimiento.Guid, result.Guid);
        
        _repositoryMock.Verify(repo => repo.GetMovimientoByGuidAsync(_movimiento1.Guid), Times.Once);
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
    
    /*[Test]
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

}*/