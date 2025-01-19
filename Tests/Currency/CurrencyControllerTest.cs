using ApiFranfurkt.Properties.Currency.Controller;
using ApiFranfurkt.Properties.Currency.Services;
using ApiFrankfurt.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Refit;

namespace Test.Currency;

[TestFixture]
public class CurrencyControllerTests
{
    private Mock<ICurrencyApiService> _currencyApiServiceMock;
    private CurrencyController _controller;

    [SetUp]
    public void SetUp()
    {
        _currencyApiServiceMock = new Mock<ICurrencyApiService>();

        var mockApiResponse = new ApiResponse<ExchangeRateResponse>(
            new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK),
            new ExchangeRateResponse(),
            null
        );

        _currencyApiServiceMock
            .Setup(service => service.GetLatestRatesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(mockApiResponse);

        _controller = new CurrencyController(_currencyApiServiceMock.Object);
    }
    

    [Test]
    public async Task GetLatestRates_ShouldReturnBadRequest_WhenAmountIsInvalid()
    {
        // Act
        var result = await _controller.GetLatestRates(amount: "invalid", baseCurrency: "USD");

        // Assert
        ClassicAssert.AreEqual(typeof(BadRequestObjectResult), result.GetType());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid amount. The value must be a positive number.", badRequestResult.Value);
    }

    [Test]
    public async Task GetLatestRates_ShouldReturnBadRequest_WhenBaseCurrencyIsEmpty()
    {
        // Act
        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "");

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestObjectResult>()); // Verificar el tipo del resultado
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid base currency. Please provide a valid currency code.", badRequestResult.Value);
    }

    [Test]
    public async Task GetLatestRates_ShouldReturnBadRequest_WhenSymbolsAreInvalid()
    {
        // Act
        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "USD", symbols: "USD,  ,EUR");

        // Assert
        ClassicAssert.AreEqual(typeof(BadRequestObjectResult), result.GetType());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid symbols parameter. Please provide valid currency codes separated by commas.",
            badRequestResult.Value);
    }
    
    

}