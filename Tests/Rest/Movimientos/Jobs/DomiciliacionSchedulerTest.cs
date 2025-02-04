using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using Quartz;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Jobs;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace Tests.Rest.Movimientos.Jobs;

[TestFixture]
[TestOf(typeof(DomiciliacionScheduler))]
public class DomiciliacionSchedulerTest
    {
        private Mock<IDomiciliacionRepository> _mockDomiciliacionRepository;
        private Mock<IMovimientoRepository> _mockMovimientoRepository;
        private Mock<IAccountsService> _mockAccountsService;
        private Mock<IUserService> _mockUserService;
        private Mock<IClientService> _mockClientService;
        private Mock<ILogger<DomiciliacionScheduler>> _mockLogger;
        private Mock<IWebsocketHandler> _mockWebsocketHandler;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IJobExecutionContext> _mockJobExecutionContext;
        private DomiciliacionScheduler _scheduler;

[SetUp]
public void SetUp()
{
    _mockDomiciliacionRepository = new Mock<IDomiciliacionRepository>();
    _mockMovimientoRepository = new Mock<IMovimientoRepository>();
    _mockAccountsService = new Mock<IAccountsService>();
    _mockUserService = new Mock<IUserService>();
    _mockClientService = new Mock<IClientService>();
    _mockLogger = new Mock<ILogger<DomiciliacionScheduler>>();
    _mockWebsocketHandler = new Mock<IWebsocketHandler>();
    _mockServiceProvider = new Mock<IServiceProvider>();
    _mockJobExecutionContext = new Mock<IJobExecutionContext>();

    var mockServiceScope = new Mock<IServiceScope>();
    var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
    mockServiceScopeFactory.Setup(x => x.CreateScope()).Returns(mockServiceScope.Object);

    _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(mockServiceScopeFactory.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IDomiciliacionRepository))).Returns(_mockDomiciliacionRepository.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IMovimientoRepository))).Returns(_mockMovimientoRepository.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IAccountsService))).Returns(_mockAccountsService.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IUserService))).Returns(_mockUserService.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IClientService))).Returns(_mockClientService.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(ILogger<DomiciliacionScheduler>))).Returns(_mockLogger.Object);
    _mockServiceProvider.Setup(x => x.GetService(typeof(IWebsocketHandler))).Returns(_mockWebsocketHandler.Object);

    _mockJobExecutionContext.Setup(x => x.JobDetail.JobDataMap["ServiceProvider"]).Returns(_mockServiceProvider.Object);

    _scheduler = new DomiciliacionScheduler(
        _mockDomiciliacionRepository.Object,
        _mockMovimientoRepository.Object,
        _mockAccountsService.Object,
        _mockUserService.Object,
        _mockClientService.Object,
        _mockLogger.Object,
        _mockWebsocketHandler.Object
    );
}



/*[Test]
public async Task Execute_ProcessesDomiciliaciones()
{
    // Arrange
    var domiciliaciones = new List<Domiciliacion>
    {
        new Domiciliacion { ClienteGuid = "client-guid-1", IbanOrigen = "ES0000000001", Cantidad = 100, Periodicidad = Periodicidad.MENSUAL, UltimaEjecucion = DateTime.UtcNow.AddMonths(-1) }
    };

    var accountResponse = new AccountCompleteResponse { Balance = 200, ClientID = "client-guid-1" };
    var clientResponse = new ClientResponse { UserId = "user-id-1" };
    var userResponse = new UserResponse { Id = "user-id-1" };
    var movimiento = new Movimiento { ClienteGuid = "client-guid-1", Domiciliacion = domiciliaciones[0] };
    var notification = new Notification<Movimiento>
    {
        Type = Notification<Movimiento>.NotificationType.Execute.ToString(),
        CreatedAt = DateTime.Now,
        Data = movimiento
    };

    _mockDomiciliacionRepository.Setup(x => x.GetAllDomiciliacionesActivasAsync()).ReturnsAsync(domiciliaciones);
    _mockAccountsService.Setup(x => x.GetCompleteAccountByIbanAsync(It.IsAny<string>())).ReturnsAsync(accountResponse);
    _mockClientService.Setup(x => x.GetClientByIdAsync(It.IsAny<string>())).ReturnsAsync(clientResponse);
    _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(userResponse);
    _mockWebsocketHandler.Setup(x => x.NotifyUserAsync<Movimiento>(It.IsAny<string>(), It.IsAny<Notification<Movimiento>>())).Returns(Task.CompletedTask);

    _mockMovimientoRepository.Setup(x => x.AddMovimientoAsync(It.IsAny<Movimiento>())).ReturnsAsync(movimiento);
    _mockDomiciliacionRepository.Setup(x => x.UpdateDomiciliacionAsync(It.IsAny<string>(), It.IsAny<Domiciliacion>())).ReturnsAsync(domiciliaciones[0]);
    
    // Act
    await _scheduler.Execute(_mockJobExecutionContext.Object);
    
    // Assert
    _mockDomiciliacionRepository.Verify(x => x.GetAllDomiciliacionesActivasAsync(), Times.Once);
    _mockAccountsService.Verify(x => x.GetCompleteAccountByIbanAsync("ES0000000001"), Times.Once);
    //_mockClientService.Verify(x => x.GetClientByIdAsync("client-guid-1"), Times.Once);
    //_mockUserService.Verify(x => x.GetUserByIdAsync("user-id-1"), Times.Once);
    //_mockMovimientoRepository.Verify(x => x.AddMovimientoAsync(It.IsAny<Movimiento>()), Times.Once);
    //_mockDomiciliacionRepository.Verify(x => x.UpdateDomiciliacionAsync(It.IsAny<string>(), It.IsAny<Domiciliacion>()), Times.Once);
    //_mockWebsocketHandler.Verify(x => x.NotifyUserAsync("user-id-1", It.IsAny<Notification<Movimiento>>()), Times.Once);
}*/




        [Test]
        public void RequiereEjecucion_ReturnsTrueForDomiciliacionNeedingExecution()
        {
            // Arrange
            var domiciliacion = new Domiciliacion
            {
                Periodicidad = Periodicidad.MENSUAL,
                UltimaEjecucion = DateTime.UtcNow.AddMonths(-1)
            };

            // Act
            var methodInfo = typeof(DomiciliacionScheduler).GetMethod("RequiereEjecucion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (bool)methodInfo.Invoke(_scheduler, new object[] { domiciliacion, DateTime.UtcNow });

            // Assert
            ClassicAssert.IsTrue(result);
        }

        [Test]
        public void RequiereEjecucion_ReturnsFalseForDomiciliacionNotNeedingExecution()
        {
            // Arrange
            var domiciliacion = new Domiciliacion
            {
                Periodicidad = Periodicidad.MENSUAL,
                UltimaEjecucion = DateTime.UtcNow
            };

            // Act
            var methodInfo = typeof(DomiciliacionScheduler).GetMethod("RequiereEjecucion", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (bool)methodInfo.Invoke(_scheduler, new object[] { domiciliacion, DateTime.UtcNow });

            // Assert
            ClassicAssert.IsFalse(result);
        }

    }