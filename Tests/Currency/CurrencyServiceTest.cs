using System.Net;
using ApiFranfurkt.Properties.Currency.Exceptions;
using ApiFranfurkt.Properties.Currency.Services;
using ApiFrankfurt.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework.Legacy;
using Refit;
using ICurrencyApiService = ApiFranfurkt.Properties.Currency.Services.ICurrencyApiService;

namespace Test.Currency;

[TestFixture]
public class CurrencyApiServiceTests
{
    private readonly Mock<ICurrencyApiService> _mockApiClient;
    private readonly Mock<ILogger<CurrencyApiService>> _mockLogger;
    private readonly CurrencyApiService _service;
    
    public CurrencyApiServiceTests()
    {
        _mockApiClient = new Mock<ICurrencyApiService>();
        _mockLogger = new Mock<ILogger<CurrencyApiService>>();
        
        _service = new CurrencyApiService(_mockApiClient.Object, _mockLogger.Object);
    }
    
    // Caso ok.
    
    [Test]
    public async Task GetLatestRatesAsyncOk()
    {
        var expectedResponse = new ApiResponse<ExchangeRateResponse>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new ExchangeRateResponse
            {
                Base = "USD",
                Rates = new Dictionary<string, double>
                {
                    { "EUR", 0.85 },
                    { "GBP", 0.75 }
                },
                Date = "2025-01-01"
            },
            new RefitSettings()
        );

        _mockApiClient
            .Setup(client => client.GetLatestRatesAsync("USD", "EUR,GBP"))
            .ReturnsAsync(expectedResponse);
        
        var response = await _service.GetLatestRatesAsync("USD", "EUR,GBP");

        ClassicAssert.True(response.IsSuccessStatusCode);
        ClassicAssert.NotNull(response.Content);
        ClassicAssert.AreEqual("USD", response.Content.Base); 
        ClassicAssert.AreEqual(0.85, response.Content.Rates["EUR"]);
        ClassicAssert.AreEqual(0.75, response.Content.Rates["GBP"]); 
    }
    
    // Respuesta vacia.
    
    [Test]
    public async Task GetLatestRatesAsyncApiEmpty()
    {
        var emptyRatesResponse = new ApiResponse<ExchangeRateResponse>(
            new HttpResponseMessage(HttpStatusCode.OK),
            new ExchangeRateResponse
            {
                Base = "USD",
                Rates = new Dictionary<string, double>(), 
                Date = "2025-01-01"
            },
            new RefitSettings()
        );

        _mockApiClient
            .Setup(client => client.GetLatestRatesAsync("USD", "EUR,GBP"))
            .ReturnsAsync(emptyRatesResponse);
        
        var response = await _service.GetLatestRatesAsync("USD", "EUR,GBP");

        ClassicAssert.NotNull(response.Content);
        ClassicAssert.AreEqual("USD", response.Content.Base);
        ClassicAssert.IsEmpty(response.Content.Rates); 
    }
    
    // Excepcion al llamar a la Api.
    
    [Test]
    public async Task GetLatestRatesAsyncIsUnsuccessful()
    {
        var errorResponse = new ApiResponse<ExchangeRateResponse>(
            new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError),
            null,
            new RefitSettings()
        );

        _mockApiClient
            .Setup(client => client.GetLatestRatesAsync("USD", "EUR"))
            .ReturnsAsync(errorResponse);
    
        var exception = Assert.ThrowsAsync<CurrencyConnectionException>(async () =>
        {
            await _service.GetLatestRatesAsync("USD", "EUR");
        });

        StringAssert.Contains("Error connecting to API", exception.Message);
        StringAssert.Contains("500", exception.Message);
    }
}