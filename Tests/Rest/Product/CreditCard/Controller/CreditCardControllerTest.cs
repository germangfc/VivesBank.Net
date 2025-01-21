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

        _mockService.Setup(service => service.GetAllCreditCardAdminAsync()).ReturnsAsync(cards);

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
    public async Task UpdateCardAsyncReturnsCreated()
    {
        var cardId = "1";
        var updateRequest = new CreditCardUpdateRequest { CardNumber = "5678" };
        var updatedCard = new CreditCardClientResponse { Id = "1", CardNumber = "5678" };

        _mockService.Setup(service => service.UpdateCreditCardAsync(cardId, updateRequest)).ReturnsAsync(updatedCard);

        var result = await _controller.UpdateCardAsync(cardId, updateRequest);

        var createdAtActionResult = result.Result as CreatedAtActionResult;
        ClassicAssert.IsNotNull(createdAtActionResult);
        ClassicAssert.AreEqual(201, createdAtActionResult.StatusCode);
        ClassicAssert.AreEqual("GetCardByIdAdminAsync", createdAtActionResult.ActionName);
        ClassicAssert.AreEqual(updatedCard, createdAtActionResult.Value);
    }
    
    [Test]
    public async Task UpdateCardAsyncNotExists()
    {
        var cardId = "99"; 
        var updateRequest = new CreditCardUpdateRequest { CardNumber = "5678" };

        _mockService.Setup(service => service.UpdateCreditCardAsync(cardId, updateRequest))
            .ReturnsAsync((CreditCardClientResponse?)null); 

        var result = await _controller.UpdateCardAsync(cardId, updateRequest);

        var notFoundResult = result.Result as NotFoundResult;
        ClassicAssert.IsNotNull(notFoundResult);
        ClassicAssert.AreEqual(404, notFoundResult.StatusCode); 
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
    
}