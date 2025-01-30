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
using VivesBankApi.Rest.Clients.Models;
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
            NullLogger<GenericStorageJson<Client>>.Instance
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
        ClassicAssert.NotNull(result);
        ClassicAssert.NotNull(result.Value);

        var pageResponse = result.Value;
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
    public async Task UpdateClient_ReturnsOkResult_WhenClientExists()
    {
        // Arrange
        var request = new ClientUpdateRequest { FullName = "Updated Client" };
        var updatedClient = new ClientResponse { Id = "1", Fullname = "Updated Client" };
        
        _service.Setup(s => s.UpdateClientAsync("1", request)).ReturnsAsync(updatedClient);

        // Act
        var result = await _clientController.UpdateClient("1", request);

        // Assert
        ClassicAssert.NotNull(result.Result);
        ClassicAssert.IsInstanceOf<OkObjectResult>(result.Result);
        var updatedClientResult = result.Result as OkObjectResult;
        ClassicAssert.NotNull(updatedClientResult.Value);
        ClassicAssert.AreEqual(updatedClient, updatedClientResult.Value);
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
        ClassicAssert.AreEqual(returnedClient[0].FullName, client.Fullname);
    }
    
    [Test]
    public async Task ExportMeData_Returns_500_WhenFileDoesNotExist(){
        
    }
}
