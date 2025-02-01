using System.Reactive.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
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

using VivesBankApi.Rest.Product.CreditCard.Models;

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
    public async Task GetAllCreditCardAdminAsync_ReturnsMappedCreditCardAdminResponse()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 10;
        string fullName = "John Doe";
        bool? isDeleted = false;
        string direction = "asc";

        var cardsFromRepo = new PagedList<CreditCardAdminResponse>(
            new List<CreditCardAdminResponse>
            {
                new CreditCardAdminResponse { Id = "1", CardNumber = "1234567890123456"},
                new CreditCardAdminResponse { Id = "2", CardNumber = "9876543210987654"}
            },
            1,
            pageNumber,
            pageSize
        );

        _mockService
            .Setup(servie => servie.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(cardsFromRepo);

        // Act
        var result = await _controller.GetAllCardsAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        var creditCardResponse = okResult.Value as List<CreditCardAdminResponse>;
        ClassicAssert.IsNotNull(creditCardResponse);
        var firstCard = creditCardResponse.First();
        ClassicAssert.AreEqual("1234567890123456", firstCard.CardNumber);
        ClassicAssert.False(firstCard.IsDeleted);
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
    public async Task ImportCreditCardsFromJson_WhenValidFile_ReturnsOkResult()
    {
        // Arrange
        var credicCards = new List<CreditCard>
        {
            new CreditCard { Id = "1", CardNumber = "1234567890123456", Pin = "123" }
        };
        _mockService.Setup(s => s.Import(It.IsAny<IFormFile>()))
        .Returns(Observable.Create<CreditCard>(observer =>
        {
            foreach (var card in credicCards)
            {
                observer.OnNext(card);  
            }
            observer.OnCompleted();
            return () => { };
        }));

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(_ => _.Length).Returns(10);
        var result = await _controller.ImportCreditCardsFromJson(fileMock.Object);
        
        // Assert
        ClassicAssert.IsInstanceOf<ObjectResult>(result);  
        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        var creditCards = okResult.Value as List<CreditCard>;
        ClassicAssert.IsNotNull(creditCards);
        ClassicAssert.AreEqual(1, creditCards.Count);
        ClassicAssert.AreEqual("1", creditCards[0].Id);
        ClassicAssert.AreEqual("1234567890123456", creditCards[0].CardNumber);
        ClassicAssert.AreEqual("123", creditCards[0].Pin);
    }

    [Test]
    public async Task ImportCreditCardsFromJson_WhenInvalidFile_ReturnsBadRequestResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(_ => _.Length).Returns(0);

        // Act
        var result = await _controller.ImportCreditCardsFromJson(fileMock.Object);

        // Assert
        ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
    }
    
    [Test]
    public async Task ExportCreditCardsToJson_ReturnsFile_WhenAsFileIsTrue()
    {
        // Arrange
        var creditCardsAdminResponse = new List<CreditCardAdminResponse>
        {
            new CreditCardAdminResponse { Id = "1", CardNumber = "1234567890123456", ExpirationDate = "2025-12-31" },
            new CreditCardAdminResponse { Id = "2", CardNumber = "9876543210987654", ExpirationDate = "2026-01-01" }
        };

        _mockService.Setup(s => s.GetAllCreditCardAdminAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>()))
            .ReturnsAsync(creditCardsAdminResponse);
        
        var fileStream = new FileStream("dummyPath", FileMode.OpenOrCreate);
        _mockService.Setup(s => s.Export(It.IsAny<List<CreditCard>>()))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _controller.ExportCreditCardsToJson(asFile: true);

        // Assert
        ClassicAssert.IsInstanceOf<FileStreamResult>(result);
        var fileResult = result as FileStreamResult;
        ClassicAssert.NotNull(fileResult);
        ClassicAssert.AreEqual("application/json", fileResult.ContentType);
        ClassicAssert.AreEqual("creditcards.json", fileResult.FileDownloadName);
    }
    
    public async Task ExportCreditCardsToJson_ReturnsOk_WhenNoCreditCardsFound()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllCreditCardAdminAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>()))
            .ReturnsAsync(new List<CreditCardAdminResponse>()); // Empty list of credit cards
        
        // Act
        var result = await _controller.ExportCreditCardsToJson(asFile: true);

        // Assert
        ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        ClassicAssert.AreEqual("No credit cards found", ((dynamic)okResult.Value).message);
    }

    [Test]
    public async Task ExportCreditCardsToJson_ReturnsJson_WhenAsFileIsFalse()
    {
        // Arrange
        var creditCardsAdminResponse = new List<CreditCardAdminResponse>
        {
            new CreditCardAdminResponse { Id = "1", CardNumber = "1234567890123456", ExpirationDate = "2025-12-31" }
        };

        _mockService.Setup(s => s.GetAllCreditCardAdminAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<bool?>(),
                It.IsAny<string>()))
            .ReturnsAsync(creditCardsAdminResponse);
        
        // Act
        var result = await _controller.ExportCreditCardsToJson(asFile: false);

        // Assert
        ClassicAssert.IsInstanceOf<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        ClassicAssert.IsInstanceOf<List<CreditCard>>(okResult.Value);
        ClassicAssert.AreEqual(1, ((List<CreditCard>)okResult.Value).Count);
    }
}
