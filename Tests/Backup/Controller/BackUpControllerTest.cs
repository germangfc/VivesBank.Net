using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Controllers;
using VivesBankApi.Backup.Service;
using VivesBankApi.Backup;
using Path = System.IO.Path;
using Xunit;

public class BackUpControllerTests
{
    private readonly Mock<IBackupService> _backupServiceMock;
    private readonly BackupController _controller;

    public BackUpControllerTests()
    {
        _backupServiceMock = new Mock<IBackupService>();
        _controller = new BackupController(_backupServiceMock.Object);
    }

    [Test]
    public async Task ExportToZip_ReturnsNotFound_WhenFileDoesNotExist()
    {
        _backupServiceMock.Setup(s => s.ExportToZip(It.IsAny<BackUpRequest>()))
            .ReturnsAsync((string)null);

        var result = await _controller.ExportToZip(new BackUpRequest());

        var notFoundResult = Xunit.Assert.IsType<NotFoundObjectResult>(result);
        Xunit.Assert.Contains("No se pudo generar", notFoundResult.Value.ToString());
    }

    [Test]
    public async Task ExportToZip_ReturnsOk_WhenFileExists()
    {
        var fakeFilePath = Path.GetTempFileName();
        File.WriteAllText(fakeFilePath, "dummy content");

        _backupServiceMock.Setup(s => s.ExportToZip(It.IsAny<BackUpRequest>()))
            .ReturnsAsync(fakeFilePath);

        var result = await _controller.ExportToZip(new BackUpRequest());

        var okResult = Xunit.Assert.IsType<OkObjectResult>(result);
        Xunit.Assert.Contains("Backup exportado correctamente", okResult.Value.ToString());

        File.Delete(fakeFilePath);
    }

    [Test]
    public async Task ImportFromZip_ReturnsBadRequest_WhenFileIsNotZip()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("archivo.txt");
        fileMock.Setup(f => f.Length).Returns(100);

        var result = await _controller.ImportFromZip(fileMock.Object);

        var badRequestResult = Xunit.Assert.IsType<BadRequestObjectResult>(result);
        Xunit.Assert.Contains("El archivo debe tener extensión .zip", badRequestResult.Value.ToString());
    }

    [Test]
    public async Task ImportFromZip_ReturnsOk_WhenImportSucceeds()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("backup.zip");
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[100]));

        _backupServiceMock.Setup(s => s.ImportFromZip(It.IsAny<BackUpRequest>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.ImportFromZip(fileMock.Object);

        var okResult = Xunit.Assert.IsType<OkObjectResult>(result);
        Xunit.Assert.Contains("Backup importado correctamente", okResult.Value.ToString());
    }
    
    [Test]
    public async Task ImportFromZip_ReturnsBadRequest_WhenFileIsEmpty()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("empty.zip");
        fileMock.Setup(f => f.Length).Returns(0);

        var result = await _controller.ImportFromZip(fileMock.Object);

        var badRequestResult = Xunit.Assert.IsType<BadRequestObjectResult>(result);
        Xunit.Assert.Contains("Debe proporcionar un archivo ZIP valido", badRequestResult.Value.ToString());
    }
}