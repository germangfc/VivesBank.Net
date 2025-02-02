using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Storage;
using Path = System.IO.Path;

namespace Tests.Rest.Movimientos.Storage;

[TestFixture]
[TestOf(typeof(MovimientoStoragePDF))]
public class MovimientoStoragePDFTest
{
    private Mock<ILogger<MovimientoStoragePDF>> _mockLogger;
    private MovimientoStoragePDF _pdfStorage;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<MovimientoStoragePDF>>();
        _pdfStorage = new MovimientoStoragePDF(_mockLogger.Object);
    }

    [Test]
    public async Task Export_ShouldCreatePdfFile_WhenMovimientosAreProvided()
    {
        // Arrange
        var movimientos = new List<Movimiento>
        {
            new Movimiento
                { Id = "1", ClienteGuid = "ABC123", Domiciliacion = new Domiciliacion(), CreatedAt = DateTime.UtcNow },
            new Movimiento
                { Id = "2", ClienteGuid = "XYZ456", Transferencia = new Transferencia(), CreatedAt = DateTime.UtcNow }
        };

        // Act
        var fileStream = await _pdfStorage.Export(movimientos);

        // Assert
        ClassicAssert.NotNull(fileStream, "File stream should not be null");
        ClassicAssert.True(File.Exists(fileStream.Name), "Exported PDF file should exist");

        // Cleanup
        fileStream.Close();
        File.Delete(fileStream.Name);
    }

    [Test]
    public async Task Export_ShouldCreateDirectory_IfNotExists()
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "pdf");
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }

        var movimientos = new List<Movimiento>
        {
            new Movimiento
                { Id = "1", ClienteGuid = "ABC123", Domiciliacion = new Domiciliacion(), CreatedAt = DateTime.UtcNow }
        };

        // Act
        var fileStream = await _pdfStorage.Export(movimientos);

        // Assert
        ClassicAssert.True(Directory.Exists(directoryPath), "PDF directory should be created if it does not exist");

        // Cleanup
        fileStream.Close();
        File.Delete(fileStream.Name);
    }
}