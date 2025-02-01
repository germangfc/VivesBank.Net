using Microsoft.Extensions.Logging;
using NUnit.Framework.Legacy;

namespace Tests.Rest.Product.CreditCard.Controller;

using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using VivesBankApi.Rest.Product.CreditCard.Controller;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Service;

[TestFixture]
public class CreditCardControllerTest
{
    private Mock<ICreditCardService> _mockService;
    private Mock<ILogger<CreditCardController>> _mockLogger;
    private CreditCardController _controller;


    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<ICreditCardService>();
        _mockLogger = new Mock<ILogger<CreditCardController>>();
        _controller = new CreditCardController(_mockService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAllCardsAdminAsyncReturnsOk()
    {
        var cards = new List<CreditCardAdminResponse>
        {
            new() { Id = "1", CardNumber = "1234" },
            new() { Id = "2", CardNumber = "5678" }
        };
        
        var result = await _controller.GetAllCardsAdminAsync();

        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.AreEqual(cards, okResult.Value);
    }

    [Test]
    public async Task GetCardByIdAdminAsyncReturnsOk()
    {
        var cardId = "1";
        var card = new CreditCardAdminResponse { Id = cardId, CardNumber = "1234" };

        _mockService.Setup(service => service.GetCreditCardByIdAdminAsync(cardId)).ReturnsAsync(card);

        var result = await _controller.GetCardByIdAdminAsync(cardId);

        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.AreEqual(card, okResult.Value);
    }

    [Test]
    public async Task GetCardByIdAdminAsyncNotExist()
    {
        var cardId = "99";
        _mockService.Setup(service => service.GetCreditCardByIdAdminAsync(cardId)).ReturnsAsync((CreditCardAdminResponse?)null);

        var result = await _controller.GetCardByIdAdminAsync(cardId);

        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.IsNull(okResult.Value);
    }

    [Test]
    public async Task CreateCardAsyncReturnsCreated()
    {
        var createRequest = new CreditCardRequest { CardNumber = "1234" };
        var createdCard = new CreditCardClientResponse { Id = "1", CardNumber = "1234" };

        _mockService.Setup(service => service.CreateCreditCardAsync(createRequest)).ReturnsAsync(createdCard);

        var result = await _controller.CreateCardAsync(createRequest);

        var createdAtActionResult = result.Result as CreatedAtActionResult;
        ClassicAssert.IsNotNull(createdAtActionResult);
        ClassicAssert.AreEqual(201, createdAtActionResult.StatusCode);
        ClassicAssert.AreEqual("GetCardByIdAdminAsync", createdAtActionResult.ActionName);
        ClassicAssert.AreEqual(createdCard, createdAtActionResult.Value);
    }

    
    [Test]
    public async Task DeleteCardAsyncReturnsNoContent()
    {
        var cardId = "1";
        _mockService.Setup(service => service.DeleteCreditCardAsync(cardId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteCardAsync(cardId);

        var noContentResult = result as NoContentResult;
        ClassicAssert.IsNotNull(noContentResult);
        ClassicAssert.AreEqual(204, noContentResult.StatusCode);
    }
    
    [Test]
    public async Task DeleteCardAsync_WhenCardNotExists_ReturnsNotFound()
    {
        var cardId = "99";
        _mockService.Setup(service => service.DeleteCreditCardAsync(cardId))
            .ThrowsAsync(new System.Collections.Generic.KeyNotFoundException()); 

        var result = await _controller.DeleteCardAsync(cardId);

        var notFoundResult = result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult, "Result should be of type NotFoundResult.");
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode, "StatusCode should be 404.");
    }
    
    [Test]
    public async Task Export_WhenValidList_ReturnsFileResult()
    {
        // Arrange
        var creditCards = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>
        {
            new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard
            {
                Id = "1",
                AccountId = "1",
                CardNumber = "1234567890123456",
                Pin = "123",
                Cvc = "123",
                ExpirationDate = DateOnly.FromDateTime(DateTime.Now.AddYears(3)),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false
            }
        };

        var filePath = System.IO.Path.GetTempFileName();
        var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);

        // Setup del mock: se espera que Export devuelva el fileStream
        _mockService.Setup(service => service.Export(It.IsAny<List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>>()))
            .ReturnsAsync(fileStream);

        // Act: Llamamos al método del controlador con asFile = true
        var result = await _controller.ExportCreditCardsToJson(true); // Verifica que asFile = true

        // Assert: Verificamos que el resultado sea un FileStreamResult
        Assert.That(result, Is.InstanceOf<FileStreamResult>());

        var fileResult = result as FileStreamResult;

        // Verificamos el nombre del archivo y el tipo de contenido
        Assert.That(fileResult.FileDownloadName, Does.Contain("creditcards.json"));
        Assert.That(fileResult.ContentType, Is.EqualTo("application/json"));

        // Limpiar archivo temporal (buena práctica)
        fileStream.Close();
        File.Delete(filePath);
    }
}