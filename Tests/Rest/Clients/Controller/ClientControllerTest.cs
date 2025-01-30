using System.Reactive.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Clients.Controller;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Clients.storage.JSON;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace Tests.Rest.Clients.Controller;

public class ClientControllerTest
{
    private Mock<IClientService> _service;
    private ClientStorageJson _storage;
    private Mock<ILogger<ClientController>> _logger;
    private ClientController _clientController;

    [SetUp]
    public void Setup()
    {
        _service = new Mock<IClientService>();
        _storage = new ClientStorageJson(
            NullLogger<GenericStorageJson<ClientResponse>>.Instance
        );
        _logger = new Mock<ILogger<ClientController>>();
        _clientController = new ClientController(_service.Object, _logger.Object, _storage);
    }
    
    [Test]
    public async Task GetAllUsersAsync_ReturnsPaginatedResults()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 2;
        var fullName = "Test";
        bool? isDeleted = false;
        var direction = "asc";

        var pagedList = new PagedList<ClientResponse>(
            new List<ClientResponse>
            {
                new ClientResponse { Id = "1", Fullname = "Test Client 1" },
                new ClientResponse { Id = "2", Fullname = "Test Client 2" }
            },
            totalCount: 5,
            pageNumber: pageNumber,
            pageSize: pageSize
        );

        _service.Setup(s => s.GetAllClientsAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(pagedList);

        // Act
        var result = await _clientController.GetAllUsersAsync(pageNumber, pageSize, fullName, isDeleted, direction) as ActionResult<PageResponse<ClientResponse>>;

        // Assert
        var pageResponse = result.Value;
        
        Assert.Multiple(() =>
        {
            ClassicAssert.NotNull(result);
            ClassicAssert.NotNull(result.Value);
            ClassicAssert.AreEqual(pagedList.TotalCount, pageResponse.TotalElements);
            ClassicAssert.AreEqual(pagedList.PageCount, pageResponse.TotalPages);
            ClassicAssert.AreEqual(pagedList.PageSize, pageResponse.PageSize);
            ClassicAssert.AreEqual(pagedList.PageNumber, pageResponse.PageNumber);
            ClassicAssert.AreEqual(pagedList.Count, pageResponse.TotalPageElements);
            ClassicAssert.AreEqual(pagedList.ToList(), pageResponse.Content);
            ClassicAssert.AreEqual(direction, pageResponse.Direction);
            ClassicAssert.AreEqual("fullName", pageResponse.SortBy);
            ClassicAssert.AreEqual(pagedList.IsFirstPage, pageResponse.First);
            ClassicAssert.AreEqual(pagedList.IsLastPage, pageResponse.Last);
            ClassicAssert.AreEqual(pagedList.Count == 0, pageResponse.Empty);
        });
    }

    [Test]
    public async Task GetById_ReturnsOkResult_WhenClientExists()
    {
        // Arrange
        var client = new ClientResponse { Id = "1", Fullname = "Test Client" };

        _service.Setup(s => s.GetClientByIdAsync("1")).ReturnsAsync(client);

        // Act
        var result = await _clientController.GetById("1");

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.AreEqual(client, result.Value);
    }

    [Test]
    public async Task GetById_ReturnsNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        _service.Setup(s => s.GetClientByIdAsync("1")).ReturnsAsync((ClientResponse)null);

        // Act
        var result = await _clientController.GetById("1");

        // Assert
        ClassicAssert.Null(result.Value);
    }
    
    [Test]
    public async Task CreateClientAsUser_ReturnsBadRequest_WhenModelIsInvalid()
    {
        // Arrange
        var request = new ClientRequest(); // Request vacío/inválido
        _clientController.ModelState.AddModelError("Name", "Required");

        // Act
        var result = await _clientController.CreateClientAsUser(request);

        // Assert
        ClassicAssert.NotNull(result);
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task CreateClientAsUser_ValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var clientRequest = new ClientRequest { FullName = "John Doe", Address = "123 Main St" };
        var expectedToken = "fake_jwt_token";
        _service.Setup(s => s.CreateClientAsync(clientRequest)).ReturnsAsync(expectedToken);

        // Act
        var result = await _clientController.CreateClientAsUser(clientRequest);

        // Assert
        ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        ClassicAssert.AreEqual(200, okResult.StatusCode);

        // Verifica que el objeto anónimo tenga la propiedad "client"
        var responseData = okResult.Value as dynamic;
        ClassicAssert.AreEqual(expectedToken, responseData.GetType().GetProperty("client").GetValue(responseData));
    }

    
    [Test]
    public async Task UpdateClientDniPhotoAsync_WithValidFile_ReturnsOk()
    {
        // Arrange
        var clientId = "12345";
        var fileName = "new_photo.png";

        var fileMock = new Mock<IFormFile>();
        var fileContent = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("fake image content"));
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileContent);
        fileMock.Setup(f => f.Length).Returns(fileContent.Length);
        fileMock.Setup(f => f.FileName).Returns(fileName);

        _service.Setup(s => s.UpdateClientPhotoAsync(clientId, fileMock.Object))
            .ReturnsAsync(fileName);

        // Act
        var result = await _clientController.UpdateClientDniPhotoAsync(clientId, fileMock.Object);

        // Assert
        ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual("Profile photo updated successfully", 
            okResult.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value));
        ClassicAssert.AreEqual(fileName, 
            okResult.Value?.GetType().GetProperty("fileName")?.GetValue(okResult.Value));
    }
    
    [Test]
    public async Task UpdateClientDniPhotoAsync_WithNullFile_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "12345";

        // Act
        var result = await _clientController.UpdateClientDniPhotoAsync(clientId, null);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("No file was provided or the file is empty.", 
            badRequestResult.Value);
    }

    [Test]
    public async Task UpdateClientDniPhotoAsync_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "12345";

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0); // Simula un archivo vacío

        // Act
        var result = await _clientController.UpdateClientDniPhotoAsync(clientId, fileMock.Object);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("No file was provided or the file is empty.", 
            badRequestResult.Value);
    }
    
    
    [Test]
    public async Task UpdateClient_ReturnsOkResult_WhenClientExists()
    {
        // Arrange
        var request = new ClientUpdateRequest { FullName = "Updated Client" };
        var updatedClient = new ClientResponse { Id = "1", Fullname = "Updated Client" };
        
        _service.Setup(s => s.UpdateClientAsync("1", request)).ReturnsAsync(updatedClient);

        // Act
        var result = await _clientController.UpdateClient("1", request);

        // Assert
        var updatedClientResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            ClassicAssert.NotNull(result.Result);
            ClassicAssert.IsInstanceOf<OkObjectResult>(result.Result);
            ClassicAssert.NotNull(updatedClientResult.Value);
            ClassicAssert.AreEqual(updatedClient, updatedClientResult.Value);
        });
    }
    

    [Test]
    public async Task UpdateClient_ReturnsNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        var request = new ClientUpdateRequest { FullName = "Updated Client" };

        _service.Setup(s => s.UpdateClientAsync("1", request)).ThrowsAsync(new ClientExceptions.ClientNotFoundException("1"));

        // Act
        IActionResult result = null;
        try
        {
            await _clientController.UpdateClient("1", request);
        }
        catch (ClientExceptions.ClientNotFoundException e)
        {
            result = new NotFoundObjectResult(new { error = e.Message });
        }

        // Assert
        ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
    }
    
    [Test]
    public async Task UpdateClient_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "1";
        var invalidRequest = new ClientUpdateRequest { FullName = "" }; // Assuming FullName is required
        _clientController.ModelState.AddModelError("FullName", "Required");

        // Act
        var result = await _clientController.UpdateClient(clientId, invalidRequest);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        var badRequestResult = result.Result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.That(((SerializableError)badRequestResult.Value)["FullName"] as string[], Is.EqualTo(new[] { "Required" }));
    }
    
    [Test]
    public async Task UpdateMeAsClient_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new ClientUpdateRequest { FullName = "Updated Client" };
        var updatedClient = new ClientResponse { Id = "1", Fullname = "Updated Client" };

        _service.Setup(s => s.UpdateMeAsync(request)).ReturnsAsync(updatedClient);

        // Act
        var result = await _clientController.UpdateMeAsClient(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.Multiple(() =>
        {
            ClassicAssert.NotNull(result.Result);
            ClassicAssert.IsInstanceOf<OkObjectResult>(result.Result);
            ClassicAssert.NotNull(okResult.Value);
            ClassicAssert.AreEqual(updatedClient, okResult.Value);
        });
    }
    
    [Test]
    public async Task UpdateMeAsClient_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var invalidRequest = new ClientUpdateRequest { FullName = "" }; // Assuming FullName is required
        _clientController.ModelState.AddModelError("FullName", "Required");

        // Act
        var result = await _clientController.UpdateMeAsClient(invalidRequest);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result.Result);
        var badRequestResult = result.Result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.That(((SerializableError)badRequestResult.Value)["FullName"] as string[], Is.EqualTo(new[] { "Required" }));
    }
    
    [Test]
    public async Task DeleteClient_ShouldCallLogicDeleteClientAsync_WhenCalledWithValidId()
    {
        // Arrange
        var clientId = "123";
        _service
            .Setup(service => service.LogicDeleteClientAsync(clientId))
            .Returns(Task.CompletedTask); // Simula una tarea exitosa

        // Act
        await _clientController.DeleteClient(clientId);

        // Assert
        // Verificar que LogicDeleteClientAsync haya sido llamado con el id correcto
        _service.Verify(service => service.LogicDeleteClientAsync(clientId), Times.Once);
    }

    [Test]
    public async Task DeleteClient_ReturnsNotFound_WhenClientDoesNotExist()
    {
        // Arrange
        _service.Setup(s => s.LogicDeleteClientAsync("1")).ThrowsAsync(new ClientExceptions.ClientNotFoundException("Client not found"));

        // Act
        IActionResult result = null;
        try
        {
            await _clientController.DeleteClient("1");
        }
        catch (ClientExceptions.ClientNotFoundException e)
        {
            result = new NotFoundObjectResult(new { error = e.Message });
        }

        // Assert
        ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
    }
    
    
    [Test]
    public async Task DeleteMeClient_ShouldCallDeleteMeOnService()
    {
        // Arrange
        bool deleteMeWasCalled = false;
        _service.Setup(s => s.DeleteMe())
            .Callback(() => deleteMeWasCalled = true) 
            .Returns(Task.CompletedTask);

        // Act
        await _clientController.DeleteMeClient();

        // Assert
        ClassicAssert.IsTrue(deleteMeWasCalled);
    }
    
    [Test]
    public async Task GetPhotoByFileNameAsync_WithValidFileName_ReturnsFileStreamResult()
    {
        // Arrange
        var fileName = "photo.png";
        var fileContent = "Contenido del fichero";
        var mimeType = "image/png";

        // Create a temporary file
        var tempFilePath = System.IO.Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, fileContent);
        var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);

        _service.Setup(s => s.GetFileAsync(fileName)).ReturnsAsync(fileStream);

        // Act
        var result = await _clientController.GetPhotoByFileNameAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<FileStreamResult>(result);
        var fileResult = result as FileStreamResult;
        ClassicAssert.IsNotNull(fileResult);
        ClassicAssert.AreEqual(mimeType, fileResult.ContentType);
        ClassicAssert.AreEqual(fileName, fileResult.FileDownloadName);

        // Clean up
        fileStream.Dispose();
        File.Delete(tempFilePath);
    }
    
    [Test]
    public async Task GetPhotoByFileNameAsync_WithEmptyFileName_ReturnsBadRequest()
    {
        // Arrange
        var fileName = string.Empty;

        // Act
        var result = await _clientController.GetPhotoByFileNameAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("File name must be provided.",
            badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
    }
    
    [Test]
    public async Task GetPhotoByFileNameAsync_ReturnsNotFound_WhenFileStreamIsNull()
    {
        // Arrange
        var fileName = "nonexistent.png";

        _service.Setup(s => s.GetFileAsync(fileName))
            .ReturnsAsync((FileStream)null);

        // Act
        var result = await _clientController.GetPhotoByFileNameAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual($"File with name {fileName} not found.", 
            notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
    }
    
    [Test]
    public async Task GetFileFromFtpAsync_WithValidFileName_ReturnsFileStreamResult()
    {
        // Arrange
        var fileName = "document.pdf";
        var fileContent = "Contenido del fichero";
        var mimeType = "application/pdf";

        // Crear un archivo temporal para simular el stream
        var tempFilePath = System.IO.Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, fileContent);
        var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);

        _service.Setup(s => s.GetFileFromFtpAsync(fileName))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _clientController.GetFileFromFtpAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<FileStreamResult>(result);
        var fileResult = result as FileStreamResult;
        ClassicAssert.IsNotNull(fileResult);
        ClassicAssert.AreEqual(mimeType, fileResult.ContentType);
        ClassicAssert.AreEqual(fileName, fileResult.FileDownloadName);

        // Clean up
        fileStream.Dispose();
        File.Delete(tempFilePath);
    }
    
    [Test]
    public async Task GetFileFromFtpAsync_WithNonExistentFile_ReturnsNotFound()
    {
        // Arrange
        var fileName = "nonexistent.pdf";

        _service.Setup(s => s.GetFileFromFtpAsync(fileName))
            .ReturnsAsync((FileStream)null);

        // Act
        var result = await _clientController.GetFileFromFtpAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual($"File with name {fileName} not found.", 
            notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
    }

    [Test]
    public async Task GetFileFromFtpAsync_WithEmptyFileName_ReturnsBadRequest()
    {
        // Arrange
        var fileName = string.Empty;

        // Act
        var result = await _clientController.GetFileFromFtpAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("File name must be provided.", 
            badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
    }

    
    [Test]
    public async Task DeleteFileAsync_ReturnsOkResult()
    {
        // Arrange
        var fileName = "document.pdf";

        _service.Setup(s => s.DeleteFileFromFtpAsync(fileName))
            .ReturnsAsync(true);

        // Act
        var result = await _clientController.DeleteFileAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual($"File with name {fileName} deleted successfully.", 
            okResult.Value?.GetType().GetProperty("message")?.GetValue(okResult.Value));
    }

    [Test]
    public async Task DeleteFileAsync_ReturnsNotFoundResult()
    {
        // Arrange
        var fileName = "nonexistent.pdf";

        _service.Setup(s => s.DeleteFileFromFtpAsync(fileName))
            .ReturnsAsync(false);

        // Act
        var result = await _clientController.DeleteFileAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        var notFoundResult = result as NotFoundObjectResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual($"File with name {fileName} not found.", 
            notFoundResult.Value?.GetType().GetProperty("message")?.GetValue(notFoundResult.Value));
    }

    [Test]
    public async Task DeleteFileAsync_ReturnsBadRequest()
    {
        // Arrange
        var fileName = string.Empty;

        // Act
        var result = await _clientController.DeleteFileAsync(fileName);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.IsNotNull(badRequestResult);
        ClassicAssert.AreEqual("File name must be provided.", 
            badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value));
    }
    
    [Test]
    public async Task ExportMeData_ReturnsJsonFile_WhenExporting()
    {
        // Arrange
        var client = new ClientResponse { Id = "1", Fullname = "Test Client 1" };
        _service.Setup(s => s.GettingMyClientData()).ReturnsAsync(client);
        
        // Act
        var result = await _clientController.GetMeDataAsClient();

        // Assert
        ClassicAssert.IsInstanceOf<FileStreamResult>(result);
        var fileResult = result as FileStreamResult;
        ClassicAssert.NotNull(fileResult);
        ClassicAssert.AreEqual("application/json", fileResult.ContentType);
        IFormFile file = new FormFile(fileResult.FileStream, 0, fileResult.FileStream.Length, "id_from_form", "test.json");
        var returnedClient = await _storage.Import(file).ToList();
        ClassicAssert.AreEqual(returnedClient[0].Id, client.Id);
        ClassicAssert.AreEqual(returnedClient[0].Fullname, client.Fullname);
    }
    
}
