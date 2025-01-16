using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Config;
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
    private Mock<DomiciliacionRepository> _repositoryMock;
    private Mock<ILogger<DomiciliacionService>> _loggerMock;
    private readonly IOptions<ApiConfig> _apiConfig;
    private DomiciliacionService _domiciliacionService;
    private List<Domiciliacion> _expectedDomiciliacionList;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<DomiciliacionService>>();
        _repositoryMock = new Mock<DomiciliacionRepository>(_mongoDatabaseSettings,_loggerMock);
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
    public void FindAllDomiciliaciones()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAllDomiciliacionesAsync())
            .ReturnsAsync(_expectedDomiciliacionList);

        // Test
        var list = _domiciliacionService.FindAllDomiciliacionesAsync();

        // Comprobaciones
        Assert.Multiple(() =>
        {
            ClassicAssert.IsNotNull(list);
            ClassicAssert.AreEqual(3, list.Result.Count);
        });

        // Verificamos que se ha llamado al método
        _repositoryMock.Verify(repo => repo.GetAllDomiciliacionesAsync(), Times.Once);
    }
}