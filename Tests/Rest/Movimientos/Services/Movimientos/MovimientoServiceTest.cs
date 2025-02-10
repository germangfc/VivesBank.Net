using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework.Legacy;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Tests.Rest.Movimientos.Services.Movimientos;

[TestFixture]
[TestOf(typeof(MovimientoService))]
public class MovimientoServiceTests
{
    // Datos de prueba
    private Movimiento _movimiento1 = new Movimiento
    {
        Id = "1",
        Guid = "Guid1",
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
        Guid = "Guid2",
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
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenInvalidDestinationIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom2", Cantidad = 100, IbanDestino = "INVALID_IBAN", IbanOrigen = "ES7620770024003102575766" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidDestinationIbanException>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Destination IBAN not valid: {domiciliacion.IbanDestino}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenInvalidSourceIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom3", Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "INVALID_IBAN" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidSourceIbanException>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Origin IBAN not valid: {domiciliacion.IbanOrigen}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenClientNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom4", Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync((ClientResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Client not found by id {user.Id}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenAccountNotFoundByIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom5", Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse { Id = "client1" });
        _mockAccountsService.Setup(s => s.GetAccountByIbanAsync(domiciliacion.IbanOrigen)).ReturnsAsync((AccountResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Account not found by IBAN {domiciliacion.IbanOrigen}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddDomiciliacionAsync_ShouldThrowException_WhenDuplicatedDomiciliacion()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom6", Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" };

        var existingDomiciliaciones = new List<Domiciliacion>
        {
            new Domiciliacion { IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" }
        };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse() { Id = "client1" });
        _mockAccountsService.Setup(s => s.GetAccountByIbanAsync(domiciliacion.IbanOrigen)).ReturnsAsync(new AccountResponse() { clientID = "client1" });
        _mockDomiciliacionRepository.Setup(r => r.GetDomiciliacionesActivasByClienteGiudAsync("client1")).ReturnsAsync(existingDomiciliaciones);

        // Act & Assert
        var ex = Assert.ThrowsAsync<DuplicatedDomiciliacionException>(async () => await _movimientoService.AddDomiciliacionAsync(user, domiciliacion));
        ClassicAssert.AreEqual($"Direct Debit to account with IBAN {domiciliacion.IbanDestino} already exists", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public async Task TestAddDomiciliacionAsync_ShouldReturnDomiciliacion_WhenValid()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var domiciliacion = new Domiciliacion { Id = "dom7", Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766" };
        var client = new ClientResponse() { Id = "client1" };
        var account = new AccountResponse() { clientID = "client1" };
        var existingDomiciliaciones = new List<Domiciliacion>();

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(client);
        _mockAccountsService.Setup(s => s.GetAccountByIbanAsync(domiciliacion.IbanOrigen)).ReturnsAsync(account);
        _mockDomiciliacionRepository.Setup(r => r.GetDomiciliacionesActivasByClienteGiudAsync("client1")).ReturnsAsync(existingDomiciliaciones);
        _mockDomiciliacionRepository.Setup(r => r.AddDomiciliacionAsync(domiciliacion)).ReturnsAsync(domiciliacion);

        // Act
        var result = await _movimientoService.AddDomiciliacionAsync(user, domiciliacion);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("client1", result.ClienteGuid);
        ClassicAssert.AreEqual(DateTime.UtcNow.Date, result.UltimaEjecucion.Date);
        _mockDomiciliacionRepository.Verify(r => r.AddDomiciliacionAsync(It.IsAny<Domiciliacion>()), Times.Once);
        _mockWebsocketHandler.Verify(w => w.NotifyUserAsync(user.Id, It.IsAny<Notification<Domiciliacion>>()), Times.Once);
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
    public async Task FindMovimientoByIdAsync_ShouldReturnMovimientoWhenFound()
    {
        // Arrange
        const string id = "Guid1";
        
        // Simulamos que el cache no tiene el movimiento (retorna RedisValue.Null)
        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);  // El cache no tiene el movimiento
        
        // Simulamos que el repositorio sí encuentra el movimiento (retorna _movimiento1)
        _mockMovimientoRepository.Setup(repo => repo.GetMovimientoByIdAsync(id))
            .ReturnsAsync(_movimiento1);

        // Act
        var result = await _movimientoService.FindMovimientoByIdAsync(id);

        // Assert
        ClassicAssert.AreEqual(_movimiento1, result);

        // Verificamos que la función del repositorio haya sido llamada una vez
        _mockMovimientoRepository.Verify(repo => repo.GetMovimientoByIdAsync(id), Times.Once);
        _mockCache.Verify(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);

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
        _mockCache.Verify(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);

    }

    [Test]
    public async Task FindMovimientoByGuidAsync_ShouldReturnMovimientoWhenFound()
    {
        // Arrange
        const string guid1 = "Guid1";
        
        // Simulamos que el cache no tiene el movimiento (retorna RedisValue.Null)
        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);  // El cache no tiene el movimiento
        
        // Simulamos que el repositorio sí encuentra el movimiento (retorna _movimiento1)
        _mockMovimientoRepository.Setup(repo => repo.GetMovimientoByGuidAsync(guid1))
            .ReturnsAsync(_movimiento1);

        // Act
        var result = await _movimientoService.FindMovimientoByGuidAsync(guid1);

        // Assert
        ClassicAssert.AreEqual(_movimiento1, result);

        // Verificamos que la función del repositorio haya sido llamada una vez
        _mockMovimientoRepository.Verify(repo => repo.GetMovimientoByGuidAsync(guid1), Times.Once);
        _mockCache.Verify(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);

    }
    
    [Test]
    public async Task FindMovimientoByGuidAsync_ShouldThrowException_WhenNotFound()
    {
        // Arrange
        const string guid = "xxx";

        // Simulamos que el repositorio no encuentra el movimiento (retorna null)
        _mockMovimientoRepository.Setup(repo => repo.GetMovimientoByGuidAsync(guid))
            .ReturnsAsync((Movimiento)null);

        // Simulamos que el cache no tiene el movimiento (retorna RedisValue.Null)
        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);  // El cache no tiene el movimiento

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
            await _movimientoService.FindMovimientoByGuidAsync(guid));

        // Assert
        ClassicAssert.AreEqual($"Movement not found with ID/Guid {guid}", ex.Message);

        _mockMovimientoRepository.Verify(repo => repo.GetMovimientoByGuidAsync(guid), Times.Once);
        _mockCache.Verify(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()), Times.Once);
    }
    
    [Test]
    public async Task UpdateMovimientoAsync_ShouldReturnUpdatedMovimiento_WhenUpdatedSuccessfully()
    {
        // Arrange
        const string id = "1";
        var updatedMovimiento = new Movimiento
        {
            Id = id,
        };

        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)JsonSerializer.Serialize(_movimiento1)); // Cache existente

        _mockMovimientoRepository.Setup(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento))
            .ReturnsAsync(_movimiento1);

        // Act;
        var result = await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento);

        // Assert
        ClassicAssert.AreEqual(_movimiento1, result);
        _mockCache.Verify(c => c.KeyDeleteAsync(id, CommandFlags.None), Times.Once);
        _mockCache.Verify(c => c.KeyDeleteAsync(_movimiento1.Guid, CommandFlags.None), Times.Once);
        _mockMovimientoRepository.Verify(repo => repo.UpdateMovimientoAsync(id,updatedMovimiento), Times.Once);

        //_mockCache.Verify(c => c.StringSetAsync(id, JsonSerializer.Serialize(_movimiento1), It.IsAny<TimeSpan>(), CommandFlags.None), Times.Once);
        //_mockCache.Verify(c => c.StringSetAsync(_movimiento1.Guid, JsonSerializer.Serialize(_movimiento1), It.IsAny<TimeSpan>(), CommandFlags.None), Times.Once);
    }

    [Test]
    public async Task UpdateMovimientoAsync_ShouldThrowException_WhenNotFound()
    {
        // Arrange
        const string id = "xxx";
        var updatedMovimiento = new Movimiento
        {
            Id = id,
        };

        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null); // Cache no existente

        _mockMovimientoRepository.Setup(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento))
            .ReturnsAsync((Movimiento)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
            await _movimientoService.UpdateMovimientoAsync(id, updatedMovimiento));

        // Assert: Verificamos que el mensaje de la excepción es el esperado
        ClassicAssert.AreEqual($"Movement not found with ID/Guid {id}", ex.Message);
        
        _mockMovimientoRepository.Verify(repo => repo.UpdateMovimientoAsync(id, updatedMovimiento), Times.Exactly(0));
        _mockCache.Verify(c => c.KeyDeleteAsync(id, CommandFlags.None), Times.Never);
        _mockCache.Verify(c => c.KeyDeleteAsync(_movimiento1.Guid, CommandFlags.None), Times.Never);
        _mockCache.Verify(c => c.StringSetAsync(id, JsonConvert.SerializeObject(updatedMovimiento), TimeSpan.FromMinutes(10),When.NotExists, CommandFlags.None), Times.Never);

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
    public async Task DeleteMovimientoAsync_ShouldThrowException_WhenNotFound()
    {
        // Arrange
        const string id = "xxx";

        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null); // Cache no existente

        _mockMovimientoRepository.Setup(repo => repo.DeleteMovimientoAsync(id))
            .ReturnsAsync((Movimiento)null); // repo no existente

        // Act & Assert
        var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
            await _movimientoService.DeleteMovimientoAsync(id));

        // Assert: Verificamos que el mensaje de la excepción es el esperado
        ClassicAssert.AreEqual($"Movement not found with ID/Guid {id}", ex.Message);
        
        _mockMovimientoRepository.Verify(repo => repo.DeleteMovimientoAsync(id), Times.Exactly(0));
        _mockCache.Verify(c => c.KeyDeleteAsync(id, CommandFlags.None), Times.Never);
        _mockCache.Verify(c => c.KeyDeleteAsync(_movimiento1.Guid, CommandFlags.None), Times.Never);
        
    }
    [Test]
    public async Task EnviarNotificacionCreacionAsync_ShouldNotifyUser_WhenNotificationSent()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var newMovimiento = new Movimiento
        {
            Id = "1",
            Guid = "Guid1",
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
            Guid = "Guid1",
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

    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenInvalidAmount()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = -10, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "A12345678" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<IngresoNominaInvalidAmountException>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Invalid Payroll Income amount ({ingresoDeNomina.Cantidad})", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenInvalidDestinationIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "INVALID_IBAN", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "A12345678" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidDestinationIbanException>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Destination IBAN not valid: {ingresoDeNomina.IbanDestino}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenInvalidSourceIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "INVALID_IBAN", CifEmpresa = "A12345678" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidSourceIbanException>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Origin IBAN not valid: {ingresoDeNomina.IbanOrigen}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenInvalidCif()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "INVALID_CIF" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidCifException>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Invalid CIF: {ingresoDeNomina.CifEmpresa}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenClientNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "B76543214" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync((ClientResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Client not found by id {user.Id}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddIngresoDeNominaAsync_ShouldThrowException_WhenAccountNotFoundByIban()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "B76543214" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse() { Id = "client1" });
        _mockAccountsService.Setup(s => s.GetCompleteAccountByIbanAsync(ingresoDeNomina.IbanDestino)).ReturnsAsync((AccountCompleteResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () => await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina));
        ClassicAssert.AreEqual($"Account not found by IBAN {ingresoDeNomina.IbanDestino}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public async Task TestAddIngresoDeNominaAsync_ShouldReturnMovimiento_WhenValid()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var ingresoDeNomina = new IngresoDeNomina { Cantidad = 100, IbanDestino = "ES6621000418401234567891", IbanOrigen = "ES7620770024003102575766", CifEmpresa = "B76543214" };
        var client = new ClientResponse() { Id = "client1" };
        var account = new AccountCompleteResponse() { ClientID = "client1", Balance = 1000 };
        var updateAccountRequest = new UpdateAccountRequest { Balance = 1100 };
        var updatedAccount = new AccountCompleteResponse() { ClientID = "client1", Balance = 1100 };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(client);
        _mockAccountsService.Setup(s => s.GetCompleteAccountByIbanAsync(ingresoDeNomina.IbanDestino)).ReturnsAsync(account);
        _mockAccountsService.Setup(s => s.UpdateAccountAsync(client.Id, It.IsAny<UpdateAccountRequest>())).ReturnsAsync(updatedAccount);
        _mockMovimientoRepository.Setup(r => r.AddMovimientoAsync(It.IsAny<Movimiento>())).ReturnsAsync(new Movimiento { ClienteGuid = "client1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false });

        // Act
        var result = await _movimientoService.AddIngresoDeNominaAsync(user, ingresoDeNomina);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("client1", result.ClienteGuid);
        ClassicAssert.AreEqual(DateTime.UtcNow.Date, result.CreatedAt.Value.Date);
        ClassicAssert.AreEqual(DateTime.UtcNow.Date, result.UpdatedAt.Value.Date);
        _mockMovimientoRepository.Verify(r => r.AddMovimientoAsync(It.IsAny<Movimiento>()), Times.Once);
        _mockWebsocketHandler.Verify(w => w.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()), Times.Once);
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenInvalidAmount()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = -10, NumeroTarjeta = "4111111111111111" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<PagoTarjetaInvalidAmountException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Invalid Card payment amount ({pagoConTarjeta.Cantidad})", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenInvalidCardNumber()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 100, NumeroTarjeta = "INVALID_CARD_NUMBER" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidCardNumberException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Invalid card number: {pagoConTarjeta.NumeroTarjeta}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenClientNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 100, NumeroTarjeta = "4111111111111111" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync((ClientResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Client not found by id {user.Id}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenCreditCardNotFound()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 100, NumeroTarjeta = "4111111111111111" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse() { Id = "client1" });
        _mockCreditCardService.Setup(s => s.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta)).ReturnsAsync((CreditCardAdminResponse)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<PagoTarjetaCreditCardNotFoundException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Credit card payment: card with card number {pagoConTarjeta.NumeroTarjeta} not found ", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenCreditCardNotAssigned()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 100, NumeroTarjeta = "1234567890123456" };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse() { Id = "client1" });
        _mockCreditCardService.Setup(s => s.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta)).ReturnsAsync(new CreditCardAdminResponse() { Id = "card1" });
        _mockAccountsService.Setup(s => s.GetCompleteAccountByClientIdAsync("client1")).ReturnsAsync(new List<AccountCompleteResponse>());

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidCardNumberException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Invalid card number: {pagoConTarjeta.NumeroTarjeta}", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public void TestAddPagoConTarjetaAsync_ShouldThrowException_WhenAccountInsufficientBalance()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 1000, NumeroTarjeta = "4111111111111111" };

        var clientAccounts = new List<AccountCompleteResponse>
        {
            new() { Id = "account1", TarjetaId = "card1", Balance = 500 }
        };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(new ClientResponse() { Id = "client1" });
        _mockCreditCardService.Setup(s => s.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta)).ReturnsAsync(new CreditCardAdminResponse() { Id = "card1" });
        _mockAccountsService.Setup(s => s.GetCompleteAccountByClientIdAsync("client1")).ReturnsAsync(clientAccounts);

        // Act & Assert
        var ex = Assert.ThrowsAsync<PagoTarjetaAccountInsufficientBalanceException>(async () => await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta));
        ClassicAssert.AreEqual($"Insufficient balance for card payment from card {pagoConTarjeta.NumeroTarjeta} ", ex.Message, "El mensaje de error no es el esperado.");
    }
    
    [Test]
    public async Task TestAddPagoConTarjetaAsync_ShouldReturnMovimiento_WhenValid()
    {
        // Arrange
        var user = new User { Id = "user1" };
        var pagoConTarjeta = new PagoConTarjeta { Cantidad = 100, NumeroTarjeta = "4111111111111111" };
        var client = new ClientResponse() { Id = "client1" };
        var card = new CreditCardAdminResponse() { Id = "card1" };
        var account = new AccountCompleteResponse() { Id = "account1", TarjetaId = "card1", Balance = 1000 };
        var updateAccountRequest = new UpdateAccountRequest { Balance = 900 };
        var updatedAccount = new AccountCompleteResponse() { Id = "account1", TarjetaId = "card1", Balance = 900 };

        _mockClientService.Setup(s => s.GetClientByUserIdAsync(user.Id)).ReturnsAsync(client);
        _mockCreditCardService.Setup(s => s.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta)).ReturnsAsync(card);
        _mockAccountsService.Setup(s => s.GetCompleteAccountByClientIdAsync(client.Id)).ReturnsAsync(new List<AccountCompleteResponse> { account });
        _mockAccountsService.Setup(s => s.UpdateAccountAsync(account.Id, It.IsAny<UpdateAccountRequest>())).ReturnsAsync(updatedAccount);
        _mockMovimientoRepository.Setup(r => r.AddMovimientoAsync(It.IsAny<Movimiento>())).ReturnsAsync(new Movimiento { ClienteGuid = client.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false });

        // Act
        var result = await _movimientoService.AddPagoConTarjetaAsync(user, pagoConTarjeta);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual("client1", result.ClienteGuid);
        ClassicAssert.AreEqual(DateTime.UtcNow.Date, result.CreatedAt.Value.Date);
        ClassicAssert.AreEqual(DateTime.UtcNow.Date, result.UpdatedAt.Value.Date);
        _mockMovimientoRepository.Verify(r => r.AddMovimientoAsync(It.IsAny<Movimiento>()), Times.Once);
        _mockWebsocketHandler.Verify(w => w.NotifyUserAsync(user.Id, It.IsAny<Notification<Movimiento>>()), Times.Once);
    }
    
    [Test]
    public async Task GetByGuidAsync_ShouldReturnMovimiento_WhenFoundInCache()
    {
        // Arrange
        var id = "test-guid";
        var cachedMovimiento = new Movimiento { Id = id, ClienteGuid = "client123" };
        var cachedMovimientoJson = JsonConvert.SerializeObject(cachedMovimiento);

       // _mockCache.Setup(c => c.StringGetAsync(id)).ReturnsAsync(cachedMovimientoJson);
        _mockCache.Setup(r => r.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)cachedMovimientoJson);
        _mockMovimientoRepository.Setup(r => r.GetMovimientoByGuidAsync(id)).ReturnsAsync((Movimiento)null);

        // Usamos reflexión para acceder al método privado
        var methodInfo = typeof(MovimientoService).GetMethod("GetByGuidAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        // Act
        var result = await (Task<Movimiento>)methodInfo.Invoke(_movimientoService, new object[] { id });

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(cachedMovimiento.Id, result.Id);
        ClassicAssert.AreEqual(cachedMovimiento.ClienteGuid, result.ClienteGuid);
    }
    
 [Test]
public async Task TestAddTransferenciaAsync_ShouldReturnMovimiento_WhenValid()
{
    // Arrange
    var user = new User { Id = "user1" };
    
    // Proporciona IBANs válidos según tu IbanValidator
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",  // IBAN válido
        IbanDestino = "ES7921000813610123456789",  // IBAN válido
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // El resto de la configuración sigue igual...
    var originClient = new ClientResponse { Id = "clientOrigin" };
    var originAccount = new AccountCompleteResponse 
    { 
        Id = "accountOrigin", 
        IBAN = "ES9121000418450200051332", 
        Balance = 200, 
        ClientID = "clientOrigin" 
    };
    var destinationAccount = new AccountCompleteResponse 
    { 
        Id = "accountDestination", 
        IBAN = "ES7921000813610123456789", 
        Balance = 300, 
        ClientID = "clientDest" 
    };
    var updatedDestinationAccount = new AccountCompleteResponse 
    { 
        Id = "accountDestination", 
        IBAN = "ES7921000813610123456789", 
        Balance = 400, 
        ClientID = "clientDest" 
    };
    var updatedOriginAccount = new AccountCompleteResponse 
    { 
        Id = "accountOrigin", 
        IBAN = "ES9121000418450200051332", 
        Balance = 100, 
        ClientID = "clientOrigin" 
    };
    var destinationClient = new ClientResponse { Id = "clientDest", UserId = "userDest" };
    var destinationUser = new User { Id = "userDest" };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync(originAccount);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES7921000813610123456789"))
        .ReturnsAsync(destinationAccount);

    _mockAccountsService
        .Setup(s => s.UpdateAccountAsync("accountDestination", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedDestinationAccount);

    _mockMovimientoRepository
        .Setup(r => r.AddMovimientoAsync(It.Is<Movimiento>(m => m.ClienteGuid == destinationAccount.ClientID)))
        .ReturnsAsync((Movimiento m) => { m.Id = "movDestId"; return m; });

    _mockClientService
        .Setup(s => s.GetClientByIdAsync("clientDest"))
        .ReturnsAsync(destinationClient);

    _mockUserService
        .Setup(s => s.GetUserByIdAsync("userDest"))
        .ReturnsAsync(destinationUser.ToUserResponse);

    _mockAccountsService
        .Setup(s => s.UpdateAccountAsync("accountOrigin", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedOriginAccount);

    _mockMovimientoRepository
        .Setup(r => r.AddMovimientoAsync(It.Is<Movimiento>(m => m.ClienteGuid == originClient.Id)))
        .ReturnsAsync((Movimiento m) => { m.Id = "movOriginId"; return m; });

    _mockWebsocketHandler
        .Setup(w => w.NotifyUserAsync("userDest", It.IsAny<Notification<Movimiento>>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _movimientoService.AddTransferenciaAsync(user, transferencia);

    // Assert
    ClassicAssert.IsNotNull(result);
    ClassicAssert.AreEqual("clientOrigin", result.ClienteGuid);
    ClassicAssert.IsNotNull(result.Transferencia);
    ClassicAssert.AreEqual("ES9121000418450200051332", result.Transferencia.IbanOrigen);
    ClassicAssert.AreEqual("ES7921000813610123456789", result.Transferencia.IbanDestino);
    ClassicAssert.AreEqual(-100, result.Transferencia.Cantidad);
    ClassicAssert.AreEqual("movDestId", result.Transferencia.MovimientoDestino);

    _mockClientService.Verify(s => s.GetClientByUserIdAsync(user.Id), Times.Once);
    _mockAccountsService.Verify(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"), Times.Once);
    _mockAccountsService.Verify(s => s.GetCompleteAccountByIbanAsync("ES7921000813610123456789"), Times.Once);
    _mockAccountsService.Verify(s => s.UpdateAccountAsync("accountDestination", It.IsAny<UpdateAccountRequest>()), Times.Once);
    _mockMovimientoRepository.Verify(r => r.AddMovimientoAsync(It.IsAny<Movimiento>()), Times.Exactly(2));
    _mockWebsocketHandler.Verify(w => w.NotifyUserAsync("userDest", It.IsAny<Notification<Movimiento>>()), Times.Once);
}

[Test]
public void TestAddTransferenciaAsync_InvalidDestinationIban_ShouldThrowException()
{
    // Arrange: Se usa un IBAN de destino inválido
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332", // IBAN válido
        IbanDestino = "INVALID_IBAN",             // IBAN inválido
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Act & Assert: Se espera que se lance la excepción por IBAN destino inválido
    var ex = Assert.ThrowsAsync<InvalidDestinationIbanException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("INVALID_IBAN"));
}
[Test]
public void TestAddTransferenciaAsync_SameIban_ShouldThrowException()
{
    // Arrange: Se utiliza el mismo IBAN para origen y destino
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES9121000418450200051332", // Mismo IBAN
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Act & Assert: Se espera que se lance la excepción por IBAN idénticos
    var ex = Assert.ThrowsAsync<TransferSameIbanException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("ES9121000418450200051332"));
}
[Test]
public void TestAddTransferenciaAsync_InvalidAmount_ShouldThrowException()
{
    // Arrange: Se define una cantidad menor o igual a cero
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 0,  // Cantidad inválida (0 o negativa)
        NombreBeneficiario = "Beneficiario"
    };

    // Act & Assert: Se espera que se lance la excepción por cantidad inválida
    var ex = Assert.ThrowsAsync<TransferInvalidAmountException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("0"));
}
[Test]
public async Task TestAddTransferenciaAsync_InsufficientBalance_ShouldThrowException()
{
    // Arrange: La cuenta origen tiene saldo insuficiente para cubrir la transferencia
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 300, // Monto mayor al saldo disponible
        NombreBeneficiario = "Beneficiario"
    };

    var originClient = new ClientResponse { Id = "clientOrigin" };
    var originAccount = new AccountCompleteResponse
    {
        Id = "accountOrigin",
        IBAN = "ES9121000418450200051332",
        Balance = 200,  // Saldo insuficiente
        ClientID = "clientOrigin"
    };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync(originAccount);

    // Act & Assert: Se espera la excepción de saldo insuficiente
    var ex = Assert.ThrowsAsync<TransferInsufficientBalance>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("ES9121000418450200051332"));
}
[Test]
public async Task TestAddTransferenciaAsync_OriginAccountNotFound_ShouldThrowException()
{
    // Arrange: Simular que la cuenta de origen no existe
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    var originClient = new ClientResponse { Id = "clientOrigin" };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync((AccountCompleteResponse)null);

    // Act & Assert: Se espera la excepción de cuenta origen no encontrada
    var ex = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("ES9121000418450200051332"));
}
[Test]
public async Task TestAddTransferenciaAsync_ClientNotFound_ShouldThrowException()
{
    // Arrange: Simular que el cliente asociado al usuario no existe
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync((ClientResponse)null);

    // Act & Assert: Se espera la excepción de cliente no encontrado
    var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring(user.Id));
}

[Test]
public void TestAddTransferenciaAsync_InvalidSourceIban_ShouldThrowException()
{
    // Arrange: Se usa un IBAN de origen inválido (suponiendo que "INVALID_IBAN" falla la validación)
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "INVALID_IBAN",             // IBAN inválido para origen
        IbanDestino = "ES7921000813610123456789",  // IBAN válido para destino
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Act & Assert
    var ex = Assert.ThrowsAsync<InvalidSourceIbanException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("INVALID_IBAN"));
}
[Test]
public void TestAddTransferenciaAsync_OriginAccountNotOwnedByClient_ShouldThrowException()
{
    // Arrange
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Cliente obtenido por el user
    var originClient = new ClientResponse { Id = "clientOrigin" };

    // La cuenta de origen existe pero no pertenece al cliente (ClientID distinto)
    var originAccount = new AccountCompleteResponse
    {
        Id = "accountOrigin",
        IBAN = "ES9121000418450200051332",
        Balance = 200,
        ClientID = "otroCliente"  // Diferente a originClient.Id
    };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync(originAccount);

    // Act & Assert: Se espera que se lance la excepción AccountsExceptions.AccountUnknownIban
    var ex = Assert.ThrowsAsync<AccountsExceptions.AccountUnknownIban>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("ES9121000418450200051332"));
}
[Test]
public void TestAddTransferenciaAsync_DestinationUserNotFound_ShouldThrowException()
{
    // Arrange
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Datos para el origen
    var originClient = new ClientResponse { Id = "clientOrigin" };
    var originAccount = new AccountCompleteResponse
    {
        Id = "accountOrigin",
        IBAN = "ES9121000418450200051332",
        Balance = 200,
        ClientID = "clientOrigin"
    };

    // Datos para el destino: la cuenta destino existe y se obtiene el cliente destino,
    // pero al buscar el usuario, se devuelve null.
    var destinationAccount = new AccountCompleteResponse
    {
        Id = "accountDestination",
        IBAN = "ES7921000813610123456789",
        Balance = 300,
        ClientID = "clientDest"
    };
    var destinationClient = new ClientResponse { Id = "clientDest", UserId = "userDest" };

    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync(originAccount);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES7921000813610123456789"))
        .ReturnsAsync(destinationAccount);

    // Se simula la actualización de la cuenta destino (aunque en este caso no se llega a completar)
    _mockAccountsService
        .Setup(s => s.UpdateAccountAsync("accountDestination", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(destinationAccount);

    // Se simula la creación del movimiento destino
    _mockMovimientoRepository
        .Setup(r => r.AddMovimientoAsync(It.Is<Movimiento>(m => m.ClienteGuid == destinationAccount.ClientID)))
        .ReturnsAsync((Movimiento m) => { m.Id = "movDestId"; return m; });

    _mockClientService
        .Setup(s => s.GetClientByIdAsync("clientDest"))
        .ReturnsAsync(destinationClient);

    // Aquí se simula que el usuario destino no se encuentra
    _mockUserService
        .Setup(s => s.GetUserByIdAsync("userDest"))
        .ReturnsAsync((UserResponse)null);

    // Act & Assert: Se espera la excepción UserNotFoundException
    var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("userDest"));
}

[Test]
public void TestAddTransferenciaAsync_DestinationClientNotFound_ShouldThrowException()
{
    // Arrange
    var user = new User { Id = "user1" };
    var transferencia = new Transferencia
    {
        IbanOrigen = "ES9121000418450200051332",
        IbanDestino = "ES7921000813610123456789",
        Cantidad = 100,
        NombreBeneficiario = "Beneficiario"
    };

    // Datos para el origen: cliente y cuenta válidos.
    var originClient = new ClientResponse { Id = "clientOrigin" };
    var originAccount = new AccountCompleteResponse
    {
        Id = "accountOrigin",
        IBAN = "ES9121000418450200051332",
        Balance = 200,
        ClientID = "clientOrigin"
    };

    // Datos para el destino: la cuenta destino existe y se obtiene, pero el cliente destino no se encuentra.
    var destinationAccount = new AccountCompleteResponse
    {
        Id = "accountDestination",
        IBAN = "ES7921000813610123456789",
        Balance = 300,
        ClientID = "clientDest"
    };

    // Configuración de mocks para el origen.
    _mockClientService
        .Setup(s => s.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(originClient);

    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES9121000418450200051332"))
        .ReturnsAsync(originAccount);

    // Configuración de mocks para el destino.
    _mockAccountsService
        .Setup(s => s.GetCompleteAccountByIbanAsync("ES7921000813610123456789"))
        .ReturnsAsync(destinationAccount);

    // Simular la actualización de la cuenta destino.
    _mockAccountsService
        .Setup(s => s.UpdateAccountAsync("accountDestination", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(destinationAccount);

    // Se simula la creación del movimiento destino (se asigna un Id al movimiento, simulando que se crea correctamente).
    _mockMovimientoRepository
        .Setup(r => r.AddMovimientoAsync(It.Is<Movimiento>(m => m.ClienteGuid == destinationAccount.ClientID)))
        .ReturnsAsync((Movimiento m) => { m.Id = "movDestId"; return m; });

    // Al buscar el cliente destino, se devuelve null para simular que no se encuentra.
    _mockClientService
        .Setup(s => s.GetClientByIdAsync("clientDest"))
        .ReturnsAsync((ClientResponse)null);

    // Act & Assert: Se espera que se lance la excepción ClientNotFoundException.
    var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
        await _movimientoService.AddTransferenciaAsync(user, transferencia)
    );

    Assert.That(ex.Message, Contains.Substring("clientDest"));
}
[Test]
public async Task RevocarTransferencia_ShouldReturnRevokedMovimiento_WhenValid()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var now = DateTime.UtcNow;

    // Movimiento original válido (creado hace 1 hora)
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        ClienteGuid = "client1",
        CreatedAt = now.AddHours(-1),
        IsDeleted = false,
        Transferencia = new Transferencia
        {
            IbanOrigen = "ESORIGEN",
            IbanDestino = "ESDESTINO",
            // Se conserva la cantidad original, por ejemplo 100
            Cantidad = 100,
            MovimientoDestino = "movDestId"
        }
    };

    // Movimiento destino (anteriormente creado en la transferencia)
    var destinationMovement = new Movimiento
    {
        Id = "movDestId",
        IsDeleted = false
    };

    // Configuramos el repositorio para obtener el movimiento original y el movimiento destino
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByIdAsync("movDestId"))
        .ReturnsAsync(destinationMovement);

    // Configuramos UpdateMovimientoAsync para devolver el objeto que se le pasa (simulando que se actualiza correctamente)
    _mockMovimientoRepository
        .Setup(m => m.UpdateMovimientoAsync(originalMovement.Id, It.IsAny<Movimiento>()))
        .ReturnsAsync((string id, Movimiento mov) => mov);
    _mockMovimientoRepository
        .Setup(m => m.UpdateMovimientoAsync(destinationMovement.Id, It.IsAny<Movimiento>()))
        .ReturnsAsync((string id, Movimiento mov) => mov);

    // El cliente que realiza la revocación (origen)
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);

    // Cuenta origen: se recupera y se actualiza (saldo: 200 - 100 = 100)
    var originAccount = new AccountCompleteResponse
    {
        Id = "originAccId",
        IBAN = "ESORIGEN",
        Balance = 200,
        ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESORIGEN"))
        .ReturnsAsync(originAccount);
    var updatedOriginAccount = new AccountCompleteResponse
    {
        Id = "originAccId",
        IBAN = "ESORIGEN",
        Balance = 100,
        ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("originAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedOriginAccount);

    // Cuenta destino: se recupera y se actualiza (saldo: 300 + 100 = 400)
    var destinationAccount = new AccountCompleteResponse
    {
        Id = "destAccId",
        IBAN = "ESDESTINO",
        Balance = 300,
        ClientID = "clientDest"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESDESTINO"))
        .ReturnsAsync(destinationAccount);
    var updatedDestinationAccount = new AccountCompleteResponse
    {
        Id = "destAccId",
        IBAN = "ESDESTINO",
        Balance = 400,
        ClientID = "clientDest"
    };
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("destAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedDestinationAccount);

    // Cliente destino y usuario destino (para notificar)
    var destinationClient = new ClientResponse { Id = "clientDest", UserId = "destUserId" };
    _mockClientService
        .Setup(c => c.GetClientByIdAsync("clientDest"))
        .ReturnsAsync(destinationClient);
    var destinationUser = new User { Id = "destUserId" };
    _mockUserService
        .Setup(u => u.GetUserByIdAsync("destUserId"))
        .ReturnsAsync(destinationUser.ToUserResponse);

    // (Opcional) Configura aquí los mocks de notificación, si son necesarios.
    // Por ejemplo:
    // _mockWebsocketHandler.Setup(w => w.NotifyUserAsync(It.IsAny<string>(), It.IsAny<Notification<Movimiento>>()))
    //                       .Returns(Task.CompletedTask);

    // Act
    var result = await _movimientoService.RevocarTransferencia(user, movimientoGuid);

    // Assert
    ClassicAssert.IsNotNull(result);
    ClassicAssert.IsTrue(result.IsDeleted);

    // Verificamos que el movimiento original haya sido actualizado
    _mockMovimientoRepository.Verify(
        m => m.UpdateMovimientoAsync("movOrigId", It.Is<Movimiento>(mov => mov.IsDeleted)),
        Times.Once);
    // Verificamos que el movimiento destino también haya sido marcado como eliminado
    _mockMovimientoRepository.Verify(
        m => m.UpdateMovimientoAsync("movDestId", It.Is<Movimiento>(mov => mov.IsDeleted)),
        Times.Once);
}



[Test]
public void RevocarTransferencia_ShouldThrowMovimientoNotFoundException_WhenMovementNotFound()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "nonexistentGuid";
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync((Movimiento)null);

    // Act & Assert
    var ex = Assert.ThrowsAsync<MovimientoNotFoundException>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring(movimientoGuid));
}
[Test]
public void RevocarTransferencia_ShouldThrowNotRevocableMovimientoException_WhenMoreThan24Hours()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        ClienteGuid = "client1",
        CreatedAt = DateTime.UtcNow.AddHours(-25),
        IsDeleted = false,
        Transferencia = new Transferencia
        {
            IbanOrigen = "ESORIGEN",
            IbanDestino = "ESDESTINO",
            Cantidad = 100,
            MovimientoDestino = null
        }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);

    // Act & Assert
    var ex = Assert.ThrowsAsync<NotRevocableMovimientoException>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring(movimientoGuid));
}
[Test]
public void RevocarTransferencia_ShouldThrowMovementIsNotTransferException_WhenTransferIsNull()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        ClienteGuid = "client1",
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        IsDeleted = false,
        Transferencia = null
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);

    // Act & Assert
    var ex = Assert.ThrowsAsync<MovementIsNotTransferException>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring(movimientoGuid));
}
[Test]
public void RevocarTransferencia_ShouldThrowClientNotFoundException_WhenClientNotFound()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        ClienteGuid = "client1",
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        IsDeleted = false,
        Transferencia = new Transferencia
        {
            IbanOrigen = "ESORIGEN",
            IbanDestino = "ESDESTINO",
            Cantidad = 100,
            MovimientoDestino = null
        }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync((ClientResponse)null);

    // Act & Assert
    var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring(user.Id));
}
[Test]
public void RevocarTransferencia_ShouldThrowAccountUnknownIban_WhenClientMismatch()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        // El movimiento fue creado por otro cliente
        ClienteGuid = "clientMismatch",
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        IsDeleted = false,
        Transferencia = new Transferencia
        {
            IbanOrigen = "ESORIGEN",
            IbanDestino = "ESDESTINO",
            Cantidad = 100,
            MovimientoDestino = null
        }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    // Se retorna un cliente cuyo Id no coincide con el del movimiento
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);

    // Act & Assert
    var ex = Assert.ThrowsAsync<AccountsExceptions.AccountUnknownIban>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring("ESORIGEN"));
}
[Test]
public void RevocarTransferencia_ShouldThrowAccountNotFoundByIban_WhenOriginAccountNotFound()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var originalMovement = new Movimiento
    {
        Id = "movOrigId",
        ClienteGuid = "client1",
        CreatedAt = DateTime.UtcNow.AddHours(-1),
        IsDeleted = false,
        Transferencia = new Transferencia
        {
            IbanOrigen = "ESORIGEN",
            IbanDestino = "ESDESTINO",
            Cantidad = 100,
            MovimientoDestino = null
        }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESORIGEN"))
        .ReturnsAsync((AccountCompleteResponse)null);

    // Act & Assert
    var ex = Assert.ThrowsAsync<AccountsExceptions.AccountNotFoundByIban>(async () =>
        await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring("ESORIGEN"));
}
[Test]
public async Task RevocarTransferencia_ShouldContinue_WhenDestinationAccountNotFound_AndLogInfo()
{
    // Arrange: Parte origen válida, pero al buscar la cuenta destino se lanza excepción.
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var now = DateTime.UtcNow;
    var originalMovement = new Movimiento
    {
         Id = "movOrigId",
         ClienteGuid = "client1",
         CreatedAt = now.AddHours(-1),
         IsDeleted = false,
         Transferencia = new Transferencia
         {
             IbanOrigen = "ESORIGEN",
             IbanDestino = "ESDESTINO",
             Cantidad = 100,
             MovimientoDestino = null
         }
    };

    // Configuramos el repositorio para obtener el movimiento original
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    // Configuramos UpdateMovimientoAsync para que devuelva el objeto actualizado
    _mockMovimientoRepository
        .Setup(m => m.UpdateMovimientoAsync(originalMovement.Id, It.IsAny<Movimiento>()))
        .ReturnsAsync((string id, Movimiento mov) => mov);

    // Cliente origen válido
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);

    // Cuenta origen: se recupera y se actualiza (saldo: 200 - 100 = 100)
    var originAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 200,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESORIGEN"))
        .ReturnsAsync(originAccount);
    var updatedOriginAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 100,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("originAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedOriginAccount);
        
    // Al buscar la cuenta destino se lanza excepción
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESDESTINO"))
        .ThrowsAsync(new AccountsExceptions.AccountNotFoundByIban("ESDESTINO"));

    // Act
    var result = await _movimientoService.RevocarTransferencia(user, movimientoGuid);

    // Assert: La revocación continúa (al menos la parte de origen) y el movimiento se marca como eliminado.
    ClassicAssert.IsNotNull(result);
    ClassicAssert.IsTrue(result.IsDeleted);
    _mockAccountsService.Verify(a => a.UpdateAccountAsync("originAccId", It.IsAny<UpdateAccountRequest>()), Times.Once);
}

[Test]
public void RevocarTransferencia_ShouldThrowClientNotFoundException_WhenDestinationClientNotFound()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var now = DateTime.UtcNow;
    var originalMovement = new Movimiento
    {
         Id = "movOrigId",
         ClienteGuid = "client1",
         CreatedAt = now.AddHours(-1),
         IsDeleted = false,
         Transferencia = new Transferencia
         {
             IbanOrigen = "ESORIGEN",
             IbanDestino = "ESDESTINO",
             Cantidad = 100,
             MovimientoDestino = null
         }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);
    var originAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 200,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESORIGEN"))
        .ReturnsAsync(originAccount);
    var updatedOriginAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 100,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("originAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedOriginAccount);
    var destinationAccount = new AccountCompleteResponse
    {
         Id = "destAccId",
         IBAN = "ESDESTINO",
         Balance = 300,
         ClientID = "clientDest"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESDESTINO"))
        .ReturnsAsync(destinationAccount);
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("destAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(destinationAccount);
    // Al obtener el cliente destino se retorna null
    _mockClientService
        .Setup(c => c.GetClientByIdAsync("clientDest"))
        .ReturnsAsync((ClientResponse)null);

    // Act & Assert
    var ex = Assert.ThrowsAsync<ClientExceptions.ClientNotFoundException>(async () =>
         await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring("clientDest"));
}
[Test]
public void RevocarTransferencia_ShouldThrowUserNotFoundException_WhenDestinationUserNotFound()
{
    // Arrange
    var user = new User { Id = "user1" };
    var movimientoGuid = "movGuid";
    var now = DateTime.UtcNow;
    var originalMovement = new Movimiento
    {
         Id = "movOrigId",
         ClienteGuid = "client1",
         CreatedAt = now.AddHours(-1),
         IsDeleted = false,
         Transferencia = new Transferencia
         {
             IbanOrigen = "ESORIGEN",
             IbanDestino = "ESDESTINO",
             Cantidad = 100,
             MovimientoDestino = null
         }
    };
    _mockMovimientoRepository
        .Setup(m => m.GetMovimientoByGuidAsync(movimientoGuid))
        .ReturnsAsync(originalMovement);
    var client = new ClientResponse { Id = "client1" };
    _mockClientService
        .Setup(c => c.GetClientByUserIdAsync(user.Id))
        .ReturnsAsync(client);
    var originAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 200,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESORIGEN"))
        .ReturnsAsync(originAccount);
    var updatedOriginAccount = new AccountCompleteResponse
    {
         Id = "originAccId",
         IBAN = "ESORIGEN",
         Balance = 100,
         ClientID = "client1"
    };
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("originAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(updatedOriginAccount);
    var destinationAccount = new AccountCompleteResponse
    {
         Id = "destAccId",
         IBAN = "ESDESTINO",
         Balance = 300,
         ClientID = "clientDest"
    };
    _mockAccountsService
        .Setup(a => a.GetCompleteAccountByIbanAsync("ESDESTINO"))
        .ReturnsAsync(destinationAccount);
    _mockAccountsService
        .Setup(a => a.UpdateAccountAsync("destAccId", It.IsAny<UpdateAccountRequest>()))
        .ReturnsAsync(destinationAccount);
    // Se retorna un cliente destino válido
    var destinationClient = new ClientResponse { Id = "clientDest", UserId = "destUserId" };
    _mockClientService
        .Setup(c => c.GetClientByIdAsync("clientDest"))
        .ReturnsAsync(destinationClient);
    // Al obtener el usuario destino se retorna null
    _mockUserService
        .Setup(u => u.GetUserByIdAsync("destUserId"))
        .ReturnsAsync((UserResponse)null);

    // Act & Assert
    var ex = Assert.ThrowsAsync<UserNotFoundException>(async () =>
         await _movimientoService.RevocarTransferencia(user, movimientoGuid));
    Assert.That(ex.Message, Contains.Substring("destUserId"));
}



}


