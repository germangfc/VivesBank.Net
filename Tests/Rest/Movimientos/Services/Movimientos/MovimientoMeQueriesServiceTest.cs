using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;


    [TestFixture]
    [TestOf(typeof(MovimientoMeQueriesService))]
    public class MovimientoMeQueriesServiceTest
    {
        private Mock<IMovimientoRepository> _repositoryMock;
        private Mock<ILogger<MovimientoMeQueriesService>> _loggerMock;
        private MovimientoMeQueriesService _movimientoMeQueriesService;

        private readonly Movimiento _movimiento1 = new()
        {
            Id = "1",
            Guid = "some-guid",
            ClienteGuid = "client-guid",
            Domiciliacion = new Domiciliacion(),
            IngresoDeNomina = new IngresoDeNomina(),
            PagoConTarjeta = new PagoConTarjeta(),
            Transferencia = new Transferencia(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        private readonly Movimiento _movimiento2 = new()
        {
            Id = "2",
            Guid = "some-guid2",
            ClienteGuid = "client-guid2",
            Domiciliacion = new Domiciliacion(),
            IngresoDeNomina = new IngresoDeNomina(),
            PagoConTarjeta = new PagoConTarjeta(),
            Transferencia = new Transferencia(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        [SetUp]
        public void Setup()
        {
            _repositoryMock = new Mock<IMovimientoRepository>();
            _loggerMock = new Mock<ILogger<MovimientoMeQueriesService>>();
            _movimientoMeQueriesService = new MovimientoMeQueriesService(_loggerMock.Object, _repositoryMock.Object);
        }

        [Test]
        public async Task FindMovimientosDomiciliacionByClienteGuidAsync_ReturnsMovimientos()
        {
            var clienteGuid = "client-guid";
            var movimientos = new List<Movimiento> { _movimiento1 };
            _repositoryMock.Setup(repo => repo.GetMovimientosDomiciliacionByClienteGuidAsync(clienteGuid))
                .ReturnsAsync(movimientos);

            var result = await _movimientoMeQueriesService.FindMovimientosDomiciliacionByClienteGuidAsync(clienteGuid);

            ClassicAssert.AreEqual(movimientos, result);
            _repositoryMock.Verify(repo => repo.GetMovimientosDomiciliacionByClienteGuidAsync(clienteGuid), Times.Once);
        }
        
        [Test]
        public async Task FindMovimientosTransferenciaByClienteGuidAsync_ReturnsMovimientos()
        {
            var clienteGuid = "client-guid";
            var movimientos = new List<Movimiento> { _movimiento1, _movimiento2 };
            _repositoryMock.Setup(repo => repo.GetMovimientosTransferenciaByClienteGuidAsync(clienteGuid))
                           .ReturnsAsync(movimientos);

            var result = await _movimientoMeQueriesService.FindMovimientosTransferenciaByClienteGuidAsync(clienteGuid);

            ClassicAssert.AreEqual(movimientos, result);
            _repositoryMock.Verify(repo => repo.GetMovimientosTransferenciaByClienteGuidAsync(clienteGuid), Times.Once);
        }

        [Test]
        public async Task FindMovimientosPagoConTarjetaByClienteGuidAsync_ReturnsMovimientos()
        {
            var clienteGuid = "client-guid";
            var movimientos = new List<Movimiento> { _movimiento1 };
            _repositoryMock.Setup(repo => repo.GetMovimientosPagoConTarjetaByClienteGuidAsync(clienteGuid))
                           .ReturnsAsync(movimientos);

            var result = await _movimientoMeQueriesService.FindMovimientosPagoConTarjetaByClienteGuidAsync(clienteGuid);

            ClassicAssert.AreEqual(movimientos, result);
            _repositoryMock.Verify(repo => repo.GetMovimientosPagoConTarjetaByClienteGuidAsync(clienteGuid), Times.Once);
        }

        [Test]
        public async Task FindMovimientosReciboDeNominaByClienteGuidAsync_ReturnsMovimientos()
        {
            var clienteGuid = "client-guid";
            var movimientos = new List<Movimiento> { _movimiento2 };
            _repositoryMock.Setup(repo => repo.GetMovimientosReciboDeNominaByClienteGuidAsync(clienteGuid))
                           .ReturnsAsync(movimientos);

            var result = await _movimientoMeQueriesService.FindMovimientosReciboDeNominaByClienteGuidAsync(clienteGuid);

            ClassicAssert.AreEqual(movimientos, result);
            _repositoryMock.Verify(repo => repo.GetMovimientosReciboDeNominaByClienteGuidAsync(clienteGuid), Times.Once);
        }
        [Test]
        public async Task FindMovimientosTransferenciaRevocadaClienteGuidAsync()
        {
            var clienteGuid = "client-guid";
            var movimientos = new List<Movimiento> { _movimiento2 };
            _repositoryMock.Setup(repo => repo.GetMovimientosTransferenciaRevocadaByClienteGuidAsync(clienteGuid))
                .ReturnsAsync(movimientos);

            var result = await _movimientoMeQueriesService.FindMovimientosTransferenciaRevocadaClienteGuidAsync(clienteGuid);

            ClassicAssert.AreEqual(movimientos, result);
            _repositoryMock.Verify(repo => repo.GetMovimientosTransferenciaRevocadaByClienteGuidAsync(clienteGuid), Times.Once);
        }
    }