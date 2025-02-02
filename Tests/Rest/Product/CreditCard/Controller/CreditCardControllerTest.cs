using System.Reactive.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;

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
    private Mock<ICreditCardService> _creditCardService;
    private Mock<ILogger<CreditCardController>> _mockLogger;
    private CreditCardController _controller;


    [SetUp]
    public void SetUp()
    {
        _creditCardService = new Mock<ICreditCardService>();
        _mockLogger = new Mock<ILogger<CreditCardController>>();
        _controller = new CreditCardController(_creditCardService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAllCardsAdminAsyncReturnsOk()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var fullName = "";
        var isDeleted = (bool?)null;
        var direction = "asc";

        var fakeCards = new List<CreditCardAdminResponse>
        {
            new CreditCardAdminResponse { Id = "1", IsDeleted = false },
            new CreditCardAdminResponse { Id = "2", IsDeleted = true }
        };

        _creditCardService
            .Setup(s => s.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(fakeCards);

        // Act
        var result = await _controller.GetAllCardsAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.NotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);

        var returnedCards = okResult.Value as List<CreditCardAdminResponse>;
        ClassicAssert.NotNull(returnedCards);
        ClassicAssert.AreEqual(2, returnedCards.Count);
    }
    
    [Test]
    public async Task GetAllCardsAdminAsync_FiltersByFullName()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var fullName = "123456";
        var isDeleted = (bool?)null;
        var direction = "asc";

        var fakeCards = new List<CreditCardAdminResponse>
        {
            new CreditCardAdminResponse { Id = "1", CardNumber = "123456", IsDeleted = false }
        };

        _creditCardService
            .Setup(s => s.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(fakeCards);

        // Act
        var result = await _controller.GetAllCardsAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.NotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);

        var returnedCards = okResult.Value as List<CreditCardAdminResponse>;
        ClassicAssert.NotNull(returnedCards);
        ClassicAssert.AreEqual(1, returnedCards.Count);
        ClassicAssert.AreEqual("123456", returnedCards[0].CardNumber);
    }
    
    [Test]
    public async Task GetAllCardsAdminAsync_FiltersByIsDeleted()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var fullName = "";
        var isDeleted = true;
        var direction = "asc";

        var fakeCards = new List<CreditCardAdminResponse>
        {
            new CreditCardAdminResponse { Id = "2", CardNumber = "1234", IsDeleted = true }
        };

        _creditCardService
            .Setup(s => s.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction))
            .ReturnsAsync(fakeCards);

        // Act
        var result = await _controller.GetAllCardsAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

        // Assert
        var okResult = result.Result as OkObjectResult;
        ClassicAssert.NotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);

        var returnedCards = okResult.Value as List<CreditCardAdminResponse>;
        ClassicAssert.NotNull(returnedCards);
        ClassicAssert.AreEqual(1, returnedCards.Count);
        ClassicAssert.IsTrue(returnedCards[0].IsDeleted);
    }
    [Test]
    public async Task GetCardByIdAdminAsyncReturnsOk()
    {
        var cardId = "1";
        var card = new CreditCardAdminResponse { Id = cardId, CardNumber = "1234" };

        _creditCardService.Setup(service => service.GetCreditCardByIdAdminAsync(cardId)).ReturnsAsync(card);

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
        _creditCardService
            .Setup(service => service.GetCreditCardByIdAdminAsync(cardId))
            .ThrowsAsync(new CreditCardException.CreditCardNotFoundException(cardId)); // Lanza excepción

        Assert.ThrowsAsync<CreditCardException.CreditCardNotFoundException>(() =>
            _controller.GetCardByIdAdminAsync(cardId));
    }

    [Test]
    public async Task GetMyCreditCards_Successfull()
    {
        var userId = "1";
        var cards = new List<CreditCardClientResponse>
        {
            new CreditCardClientResponse { Id = "1", CardNumber = "1234" },
            new CreditCardClientResponse { Id = "2", CardNumber = "5678" }
        };

        _creditCardService.Setup(service => service.GetMyCreditCardsAsync()).ReturnsAsync(cards);

        var result = await _controller.GetMyCardsAsync();

        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.AreEqual(cards, okResult.Value);
    }

    [Test]
    public async Task GetMyCreditCards_ThrowsException()
    {
        _creditCardService.Setup(service => service.GetMyCreditCardsAsync()).ThrowsAsync(new Exception("Error"));

        Assert.ThrowsAsync<Exception>(() =>
            _controller.GetMyCardsAsync());
    }
    

    [Test]
    public async Task CreateCardAsyncReturnsCreated()
    {
        var createRequest = new CreditCardRequest { CardNumber = "1234" };
        var createdCard = new CreditCardClientResponse { Id = "1", CardNumber = "1234" };

        _creditCardService.Setup(service => service.CreateCreditCardAsync(createRequest)).ReturnsAsync(createdCard);

        var result = await _controller.CreateCardAsync(createRequest);

        var createdAtActionResult = result.Result as CreatedAtActionResult;
        ClassicAssert.IsNotNull(createdAtActionResult);
        ClassicAssert.AreEqual(201, createdAtActionResult.StatusCode);
        ClassicAssert.AreEqual("GetCardByIdAdminAsync", createdAtActionResult.ActionName);
        ClassicAssert.AreEqual(createdCard, createdAtActionResult.Value);
    }

    [Test]
    public async Task CreateCardAsync_BadRequestPin_UnderLimit()
    {
        var invalidRequest = new CreditCardRequest() 
        { 
            Pin = "12", 
            AccountIban = "123456789012343"
        };
        
        _controller.ModelState.AddModelError("Pin", "The pin must be of 4 characters");
        
        var result = await _controller.CreateCardAsync(invalidRequest);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
        
        var errors = badRequestResult.Value as SerializableError;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.ContainsKey("Pin"), Is.True);
    }

    [Test]
    public async Task CreateCardAsync_BadRequestPin_OverLimit()
    {
        var invalidRequest = new CreditCardRequest() 
        { 
            Pin = "12345", 
            AccountIban = "123456789012343"
        };
        
        _controller.ModelState.AddModelError("Pin", "The pin must be of 4 characters");
        
        var result = await _controller.CreateCardAsync(invalidRequest);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
        
        var errors = badRequestResult.Value as SerializableError;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.ContainsKey("Pin"), Is.True);
    }

    [Test]
    public async Task UpdateMyCreditCard_Successfully()
    {
        var cardId = "1";
        var updateRequest = new CreditCardUpdateRequest { Pin = "1234" };
        var updatedCard = new CreditCardClientResponse { Id = cardId, Pin = "1234" };

        _creditCardService.Setup(service => service.UpdateCreditCardAsync(cardId, updateRequest)).ReturnsAsync(updatedCard);

        var result = await _controller.UpdateCardAsync(cardId, updateRequest);

        var okResult = result.Result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);
        ClassicAssert.AreEqual(200, okResult.StatusCode);
        ClassicAssert.AreEqual(updatedCard, okResult.Value);
    }

    [Test]
    public async Task UpdateMyCreditCard_ThrowsException()
    {
        var cardId = "1";
        var updateRequest = new CreditCardUpdateRequest { Pin = "1234" };
        _creditCardService.Setup(service => service.UpdateCreditCardAsync(cardId, updateRequest)).ThrowsAsync(new Exception("Error"));

        Assert.ThrowsAsync<Exception>(() =>
            _controller.UpdateCardAsync(cardId, updateRequest));
    }

    [Test]
    public async Task UpdateMyCreditCard_BadRequestPin_UnderLimit()
    {
        var cardId = "1";
        var updateRequest = new CreditCardUpdateRequest { Pin = "123" };
        _controller.ModelState.AddModelError("Pin", "The pin must be of 4 characters");
        
        var result = await _controller.UpdateCardAsync(cardId, updateRequest);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
        
        var errors = badRequestResult.Value as SerializableError;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.ContainsKey("Pin"), Is.True);
    }

    [Test]
    public async Task UpdateMyCreditCard_BadRequestPin_AboveLimit()
    {
        var cardId = "1";
        var updateRequest = new CreditCardUpdateRequest { Pin = "12345" };
        _controller.ModelState.AddModelError("Pin", "The pin must be of 4 characters");
        
        var result = await _controller.UpdateCardAsync(cardId, updateRequest);
        
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
        
        var errors = badRequestResult.Value as SerializableError;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors!.ContainsKey("Pin"), Is.True);
    }

    
    [Test]
    public async Task DeleteCardAsyncReturnsNoContent()
    {
        var cardId = "1";
        _creditCardService.Setup(service => service.DeleteCreditCardAsync(cardId)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteCardAsync(cardId);

        var noContentResult = result as NoContentResult;
        ClassicAssert.IsNotNull(noContentResult);
        ClassicAssert.AreEqual(204, noContentResult.StatusCode);
    }
    
    [Test]
    public async Task DeleteCardAsync_WhenCardNotExists_ReturnsNotFound()
    {
        var cardId = "99";
        _creditCardService.Setup(service => service.DeleteCreditCardAsync(cardId))
            .ThrowsAsync(new System.Collections.Generic.KeyNotFoundException()); 

        var result = await _controller.DeleteCardAsync(cardId);

        var notFoundResult = result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult, "Result should be of type NotFoundResult.");
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode, "StatusCode should be 404.");
    }
    
    [Test]
    public async Task ImportCreditCardsFromJson_WhenValidFile_ReturnsOkResult()
    {
           
               
            var mockFile = new Mock<IFormFile>();

            var fileContent = "[{\"Id\": \"1\", \"AccountId\": \"1\", \"CardNumber\": \"1234567890123456\", \"Pin\": \"123\", \"Cvc\": \"123\", \"ExpirationDate\": \"2025-12-31\", \"CreatedAt\": \"2023-01-01T00:00:00\", \"UpdatedAt\": \"2023-01-01T00:00:00\", \"IsDeleted\": false}]";
            var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

            mockFile.Setup(f => f.OpenReadStream()).Returns(fileStream);
            mockFile.Setup(f => f.Length).Returns(fileStream.Length);
            mockFile.Setup(f => f.FileName).Returns("creditcards.json");
            mockFile.Setup(f => f.ContentType).Returns("application/json");

            var creditCardServiceMock = new Mock<ICreditCardService>();
            var loggerMock = new Mock<ILogger<CreditCardController>>();

            var controller = new CreditCardController(creditCardServiceMock.Object, loggerMock.Object);
            
            var creditCards = new List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>
            {
                new VivesBankApi.Rest.Product.CreditCard.Models.CreditCard()
                {
                    Id = "1",
                    AccountId = "1",
                    CardNumber = "1234567890123456",
                    Pin = "123",
                    Cvc = "123",
                    ExpirationDate = DateOnly.FromDateTime(DateTime.Now)
                }
            };
            
            creditCardServiceMock.Setup(service => service.Import(mockFile.Object))
                .Returns(creditCards.ToObservable());
        
            var result = await controller.ImportCreditCardsFromJson(mockFile.Object);
            
            Assert.That(result, Is.InstanceOf<OkObjectResult>());

            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var returnedCards = okResult.Value as List<VivesBankApi.Rest.Product.CreditCard.Models.CreditCard>;
            Assert.That(returnedCards, Is.Not.Null);
            Assert.That(returnedCards.Count, Is.EqualTo(1));
            Assert.That(returnedCards[0].Id, Is.EqualTo("1"));
            Assert.That(returnedCards[0].CardNumber, Is.EqualTo("1234567890123456"));
            Assert.That(returnedCards[0].Pin, Is.EqualTo("123"));
    }
}