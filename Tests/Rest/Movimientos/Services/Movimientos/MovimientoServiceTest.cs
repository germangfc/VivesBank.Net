
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Service;
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
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace Tests.Rest.Movimientos.Services.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoService))]
public class MovimientoServiceTests
{
    // Datos de prueba
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

    // Lista esperada para pruebas
    private List<Movimiento> _expectedMovimientoList;

    private Mock<IMovimientoRepository> _mockMovimientoRepository;
    private Mock<IDomiciliacionRepository> _mockDomiciliacionRepository;
    private Mock<IUserService> _mockUserService;
    private Mock<IClientService> _mockClientService;
    private Mock<IAccountsService> _mockAccountsService;
    private Mock<ICreditCardService> _mockCreditCardService;
    private Mock<ILogger<MovimientoService>> _mockLogger;
    private Mock<IOptions<ApiConfig>> _mockApiConfig;
    private Mock<IWebsocketHandler> _mockWebsocketHandler;
    private Mock<IConnectionMultiplexer> _mockConnection;
    private Mock<IDatabase> _mockCache;

    private MovimientoService _movimientoService;

    [SetUp]
    public void SetUp()
    {
        // Inicializamos los mocks
        _mockMovimientoRepository = new Mock<IMovimientoRepository>();
        _mockDomiciliacionRepository = new Mock<IDomiciliacionRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockClientService = new Mock<IClientService>();
        _mockAccountsService = new Mock<IAccountsService>();
        _mockCreditCardService = new Mock<ICreditCardService>();
        _mockLogger = new Mock<ILogger<MovimientoService>>();
        _mockApiConfig = new Mock<IOptions<ApiConfig>>();
        _mockWebsocketHandler = new Mock<IWebsocketHandler>();
        _mockConnection = new Mock<IConnectionMultiplexer>();
        _mockCache = new Mock<IDatabase>();
        
        // Mock de IConnectionMultiplexer para que devuelva el mock de IDatabase
        _mockConnection.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockCache.Object);

        _movimientoService = new MovimientoService(
            _mockMovimientoRepository.Object,
            _mockDomiciliacionRepository.Object,
            _mockUserService.Object,
            _mockClientService.Object,
            _mockAccountsService.Object,
            _mockCreditCardService.Object,
            _mockLogger.Object,
            _mockApiConfig.Object,
            _mockWebsocketHandler.Object,
            _mockConnection.Object);

        // Lista de movimientos esperados
        _expectedMovimientoList = new List<Movimiento>
        {
            _movimiento1, _movimiento2
        };
        
    }

    [Test]
    public async Task TestFindAllMovimientosAsync_ShouldReturnMovimientos()
    {
        
        // Arrange
        _mockMovimientoRepository.Setup(repo => repo.GetAllMovimientosAsync()).ReturnsAsync(_expectedMovimientoList);

        // Act
        var result = await _movimientoService.FindAllMovimientosAsync();

        // Assert
        Assert.Multiple(() =>
        {
            // Verificamos que la lista devuelta contenga 2 movimientos
            ClassicAssert.AreEqual(_expectedMovimientoList.Count, result.Count, "El número de movimientos no es el esperado.");
            ClassicAssert.AreEqual(_expectedMovimientoList[0].Id, result[0].Id, "El ID del primer movimiento no coincide.");
            ClassicAssert.AreEqual(_expectedMovimientoList[1].Id, result[1].Id, "El ID del segundo movimiento no coincide.");
        });

        _mockMovimientoRepository.Verify(repo => repo.GetAllMovimientosAsync(), Times.Once);
    }

    [Test]
    public async Task FindAllMovimientosByClientAsyncEmptyList()
    {
        // Arrange
        const string clienteId = "10";  
        var expectedEmptyList = new List<Movimiento>();  

        _mockMovimientoRepository.Setup(repo => repo.GetMovimientosByClientAsync(clienteId))
            .ReturnsAsync(expectedEmptyList);

        // Act
        var result = await _movimientoService.FindAllMovimientosByClientAsync(clienteId);

        // Assert
        ClassicAssert.AreEqual(expectedEmptyList, result);
        
        _mockMovimientoRepository.Verify(repo => repo.GetMovimientosByClientAsync(clienteId), Times.Once);
        
    }
    
    
    
    [Test]
    public async Task FindAllMovimientosByClientAsyncOk()
    {
        // Arrange
        const string clienteId = "1";  
        _mockMovimientoRepository.Setup(repo => repo.GetMovimientosByClientAsync(clienteId))
            .ReturnsAsync(_expectedMovimientoList);  

        // Act
        var result = await _movimientoService.FindAllMovimientosByClientAsync(clienteId);

        // Assert
        ClassicAssert.AreEqual(_expectedMovimientoList, result);  
        
        _mockMovimientoRepository.Verify(repo => repo.GetMovimientosByClientAsync(clienteId), Times.Once);
    }
    

    [Test]
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenInvalidAmount()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom1", Cantidad = -10, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<DomiciliacionInvalidAmountException>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Invalid Direct Debit amount ({domiciliacion.Cantidad}), id {domiciliacion.Id}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    
    [Test]
    public async Task AddMovimientoAsyncOk()
    {
        // Arrange
        var newMovimiento = _expectedMovimientoList.First();
        _mockMovimientoRepository.Setup(repo => repo.AddMovimientoAsync(newMovimiento))
            .ReturnsAsync(newMovimiento);
        
        // Act
        var result = await _movimientoService.AddMovimientoAsync(newMovimiento);
        
        // Assert
        ClassicAssert.AreEqual(newMovimiento, result);
        
        _mockMovimientoRepository.Verify(repo => repo.AddMovimientoAsync(newMovimiento), Times.Once);
    }
    
    
        [Test]
        public async Task FindMovimientoByIdAsync_ShouldThrowException_WhenNotFound()
        {
            // Arrange
            const string id = "xxx";
    
            // Simulamos que el repositorio no encuentra el movimiento (retorna null)
            _mockMovimientoRepository.Setup(repo => repo.GetMovimientoByIdAsync(id))
                .ReturnsAsync((Movimiento)null);

            // Simulamos que el cache no tiene el movimiento (retorna RedisValue.Null)
            _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);  // El cache no tiene el movimiento

            // Act & Assert
            var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
                await _movimientoService.FindMovimientoByIdAsync(id));

            // Assert: Verificamos que el mensaje de la excepción es el esperado
            ClassicAssert.AreEqual($"Movement not found with ID/Guid {id}", ex.Message);
    
            // Verificamos que la función del repositorio haya sido llamada una vez
            _mockMovimientoRepository.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
        }

        

        [Test]
        public async Task DeleteMovimientoAsync_ShouldReturnDeletedMovimiento_WhenDeletedSuccessfully()
        {
            // Arrange
            const string id = "1";

            _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisValue)JsonSerializer.Serialize(_movimiento1)); // Cache existente

            _mockMovimientoRepository.Setup(repo => repo.DeleteMovimientoAsync(id))
                .ReturnsAsync(_movimiento1);

            // Act
            var result = await _movimientoService.DeleteMovimientoAsync(id);

            // Assert
            ClassicAssert.AreEqual(_movimiento1, result);
            _mockCache.Verify(c => c.KeyDeleteAsync(id, CommandFlags.None), Times.Once);
            _mockCache.Verify(c => c.KeyDeleteAsync(_movimiento1.Guid, CommandFlags.None), Times.Once);
            _mockMovimientoRepository.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Once);
        }

        [Test]
        public async Task EnviarNotificacionCreacionAsync_ShouldNotifyUser_WhenNotificationSent()
        {
            // Arrange
            var user = new User { Id = "user1" };
            var newMovimiento = new Movimiento
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

            // Mock del websocketHandler
            _mockWebsocketHandler.Setup(ws => ws.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _movimientoService.EnviarNotificacionCreacionAsync(user, newMovimiento);

            // Assert
            _mockWebsocketHandler.Verify(ws => ws.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()), Times.Once);
        }

        [Test]
        public async Task EnviarNotificacionDeleteAsync_ShouldNotifyUser_WhenNotificationSent()
        {
            // Arrange
            var user = new User { Id = "user1" };
            var deletedMovimiento = new Movimiento
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
                IsDeleted = true
            };

            // Mock del websocketHandler
            _mockWebsocketHandler.Setup(ws => ws.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()))
                .Returns(Task.CompletedTask);

            // Act
            await _movimientoService.EnviarNotificacionDeleteAsync(user, deletedMovimiento);

            // Assert
            _mockWebsocketHandler.Verify(ws => ws.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()), Times.Once);
        }

}


