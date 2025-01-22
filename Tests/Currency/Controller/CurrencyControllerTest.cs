using ApiFranfurkt.Properties.Currency.Controller;
using ApiFranfurkt.Properties.Currency.Services;
using ApiFrankfurt.Configuration;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        _controller = new CurrencyController(_currencyApiServiceMock.Object);
    }

    // Cantidad invalida.
    
    [Test]
    public async Task GetLatestRatesAmountIsInvalid()
    {
        var result = await _controller.GetLatestRates(amount: "invalid", baseCurrency: "USD");

        ClassicAssert.AreEqual(typeof(BadRequestObjectResult), result.GetType());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid amount. The value must be a positive number.", badRequestResult.Value);
    }

    // Moneda base vacia.
    
    [Test]
    public async Task GetLatestRatesBaseCurrencyIsEmpty()
    {
        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "");

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid base currency. Please provide a valid currency code.", badRequestResult.Value);
    }

    // Simbolo invalido.
    
    [Test]
    public async Task GetLatestRatesSymbolsInvalid()
    {
        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "USD", symbols: "USD,  ,EUR");

        ClassicAssert.AreEqual(typeof(BadRequestObjectResult), result.GetType());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid symbols parameter. Please provide valid currency codes separated by commas.",
            badRequestResult.Value);
    }

    // Caso ok.
    
    [Test]
    public async Task GetLatestRatesOk()
    {
        var mockExchangeRates = new ExchangeRateResponse
        {
            Rates = new Dictionary<string, double>
            {
                { "USD", 1.1 },
                { "EUR", 0.9 }
            },
            Base = "USD"
        };

        var mockApiResponse = new ApiResponse<ExchangeRateResponse>(
            new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK),
            mockExchangeRates,
            null
        );

        _currencyApiServiceMock
            .Setup(service =>
                service.GetLatestRatesAsync(It.Is<string>(b => b == "USD"), It.Is<string>(s => s == "EUR,USD")))
            .ReturnsAsync(mockApiResponse);

        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "USD", symbols: "EUR,USD");

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);

        var response = okResult.Value as ExchangeRateResponse;
        ClassicAssert.IsNotNull(response);

        ClassicAssert.AreEqual(11.0, response.Rates["USD"]);
        ClassicAssert.AreEqual(9.0, response.Rates["EUR"]); 
    }

    // Simbolo nulo.
    
    [Test]
    public async Task GetLatestRatesSymbolsIsNull()
    {
        var mockExchangeRates = new ExchangeRateResponse
        {
            Rates = new Dictionary<string, double>
            {
                { "USD", 1.1 },
                { "EUR", 0.9 }
            },
            Base = "USD"
        };

        var mockApiResponse = new ApiResponse<ExchangeRateResponse>(
            new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK),
            mockExchangeRates,
            null
        );

        _currencyApiServiceMock
            .Setup(service => service.GetLatestRatesAsync(It.IsAny<string>(), string.Empty))
            .ReturnsAsync(mockApiResponse);

        var result = await _controller.GetLatestRates(amount: "10", baseCurrency: "USD");

        Assert.That(result, Is.TypeOf<OkObjectResult>());

        var okResult = result as OkObjectResult;
        ClassicAssert.IsNotNull(okResult);

        var response = okResult.Value as ExchangeRateResponse;
        ClassicAssert.IsNotNull(response);

        ClassicAssert.AreEqual(11.0, response.Rates["USD"]);
        ClassicAssert.AreEqual(9.0, response.Rates["EUR"]); 
    }

    // Cantidad negativa.
    
    [Test]
    public async Task GetLatestRatesAmountIsNegative()
    {
        var result = await _controller.GetLatestRates(amount: "-10", baseCurrency: "USD");

        ClassicAssert.AreEqual(typeof(BadRequestObjectResult), result.GetType());
        var badRequestResult = result as BadRequestObjectResult;
        ClassicAssert.AreEqual("Invalid amount. The value must be a positive number.", badRequestResult.Value);
    }
}
