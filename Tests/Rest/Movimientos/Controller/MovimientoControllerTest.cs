using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Controller;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace Tests.Rest.Movimientos.Controller
{
    public class MovimientoControllerTests
    {
        private Mock<IMovimientoService> _mockMovimientoService;
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<ILogger<MovimientoController>> _mockLogger;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private MovimientoController _controller;
        private ClaimsPrincipal _user;

        private Domiciliacion _domiciliacion;
        [SetUp]
        public void SetUp()
        {
            _mockMovimientoService = new Mock<IMovimientoService>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockLogger = new Mock<ILogger<MovimientoController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            _user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));
            
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User).Returns(_user);

            _controller = new MovimientoController(
                _mockMovimientoService.Object,
                _mockUserRepository.Object,
                _mockLogger.Object,
                _mockHttpContextAccessor.Object
            );
            
            _domiciliacion = new Domiciliacion
            {
                ClienteGuid = "Guid1",
                IbanOrigen = "ES9121000418450200051332",
                IbanDestino = "ES6621000418401234567891",
                Cantidad = 100,
                NombreAcreedor = "Gas",
                FechaInicio = DateTime.UtcNow,
                Periodicidad = Periodicidad.DIARIA,
                Activa = true,
                UltimaEjecucion = DateTime.UtcNow.AddMonths(-1)
            };
        }

        [Test]
        public async Task CreateDomiciliacion_ReturnsActionResultWithDomiciliacion()
        {
            // Arrange
            
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMovimientoService.Setup(x => x.AddDomiciliacionAsync(It.IsAny<User>(), It.IsAny<Domiciliacion>()))
                .ReturnsAsync(_domiciliacion);

            // Act
            var result = await _controller.CreateDomiciliacion(_domiciliacion);

            // Assert
            ClassicAssert.AreEqual(_domiciliacion, result.Value);
            //_mockLogger.Verify(x => x.LogInformation("Creating new domiciliacion"), Times.Once);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
            _mockMovimientoService.Verify(x => x.AddDomiciliacionAsync(user, _domiciliacion), Times.Once);
        }

        [Test]
        public async Task AddTransferencia_ReturnsActionResultWithTransferencia()
        {
            // Arrange
            var transferencia = new Transferencia
            {
                IbanOrigen = "ES7600810001200000012345",
                IbanDestino = "ES9702100002750000001234",
                Cantidad = 500,
                NombreBeneficiario = "Juan Pérez",
                MovimientoDestino = "PagoFactura"
            };
            var movimiento = new Movimiento
            {
                Transferencia = transferencia
            };
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMovimientoService.Setup(x => x.AddTransferenciaAsync(It.IsAny<User>(), It.IsAny<Transferencia>()))
                .ReturnsAsync(movimiento);

            // Act
            var result = await _controller.AddTransferencia(transferencia);

            // Assert
            ClassicAssert.AreEqual(movimiento.Transferencia, result.Value.Transferencia);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
            _mockMovimientoService.Verify(x => x.AddTransferenciaAsync(user, transferencia), Times.Once);
        }


        [Test]
        public async Task AddIngresoDeNomina_ReturnsActionResultWithIngresoDeNomina()
        {
            // Arrange
            var ingresoDeNomina = new IngresoDeNomina
            {
                Cantidad = 1000,
                IbanDestino = "ES9702100002750000001234",
                CifEmpresa = "B76543214",
                NombreEmpresa = "Movistar",
            };
            var movimiento = new Movimiento
            {
                IngresoDeNomina = ingresoDeNomina
            };
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMovimientoService.Setup(x => x.AddIngresoDeNominaAsync(It.IsAny<User>(), It.IsAny<IngresoDeNomina>()))
                .ReturnsAsync(movimiento);

            // Act
            var result = await _controller.AddIngresoDeNomina(ingresoDeNomina);

            // Assert
            ClassicAssert.AreEqual(ingresoDeNomina, result.Value.IngresoDeNomina);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
            _mockMovimientoService.Verify(x => x.AddIngresoDeNominaAsync(user, ingresoDeNomina), Times.Once);
        }
        [Test]
        public async Task AddPagoConTarjeta_ReturnsActionResultWithPagoConTarjeta()
        {
            // Arrange
            var pagoConTarjeta = new PagoConTarjeta
            {
                Cantidad = 100,
                NumeroTarjeta = "1234567890123456",
                NombreComercio = "Restaurante"
            };
            var movimiento = new Movimiento
            {
                PagoConTarjeta = pagoConTarjeta
            };
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMovimientoService.Setup(x => x.AddPagoConTarjetaAsync(It.IsAny<User>(), It.IsAny<PagoConTarjeta>()))
                .ReturnsAsync(movimiento);

            // Act
            var result = await _controller.AddPagoConTarjeta(pagoConTarjeta);

            // Assert
            ClassicAssert.AreEqual(pagoConTarjeta, result.Value.PagoConTarjeta);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
            _mockMovimientoService.Verify(x => x.AddPagoConTarjetaAsync(user, pagoConTarjeta), Times.Once);
        }

        [Test]
        public async Task RevocarTransferencia_ReturnsActionResultWithMovimiento()
        {
            // Arrange
            var transfGuid = "test-guid";
            var movimiento = new Movimiento
            {
                ClienteGuid = "Guid1",
                Domiciliacion = _domiciliacion,
                IngresoDeNomina = null,
                PagoConTarjeta = null,
                Transferencia = null,
            };
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockMovimientoService.Setup(x => x.RevocarTransferenciaAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(movimiento);

            // Act
            var result = await _controller.RevocarTransferencia(transfGuid);

            // Assert
            ClassicAssert.AreEqual(movimiento, result.Value);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
            _mockMovimientoService.Verify(x => x.RevocarTransferenciaAsync(user, transfGuid), Times.Once);
        }
        
        [Test]
        public async Task ConvertClaimsPrincipalToUser_ReturnsUser()
        {
            // Arrange
            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));
    
            var user = new User { Id = "test-user-id" };
            _mockUserRepository.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
    
            // Act
            var methodInfo = typeof(MovimientoController).GetMethod("ConvertClaimsPrincipalToUser", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = (Task<User>)methodInfo.Invoke(_controller, new object[] { userClaims });
            var result = await task;

            // Assert
            ClassicAssert.AreEqual(user, result);
            _mockUserRepository.Verify(x => x.GetByIdAsync("test-user-id"), Times.Once);
        }

       

    }
    
}
