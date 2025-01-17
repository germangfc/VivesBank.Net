using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Service;

namespace VivesBankApi.Rest.Product.CreditCard.Controller;


[ApiController]
[Route("api/[controller]")]
public class CreditCardController : ControllerBase
{
    private readonly ICreditCardService _creditCardService;
    private readonly ILogger _logger;

    public CreditCardController(ICreditCardService creditCardService, ILogger<CreditCardController> logger)
    {
        _creditCardService = creditCardService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<CreditCardAdminResponse>>> GetAllCardsAdminAsync()
    {
        _logger.LogInformation("Getting all credit cards");
        var cards = await _creditCardService.GetAllCreditCardAdminAsync();
        return Ok(cards);
    }
    
    [HttpGet("{cardId}")]
    public async Task<ActionResult<CreditCardAdminResponse?>> GetCardByIdAdminAsync(string cardId)
    {
        _logger.LogInformation($"Getting card with id {cardId}");
        var card = await _creditCardService.GetCreditCardByIdAdminAsync(cardId);

        if (card == null) return NotFound();

        return Ok(card);
    
    }

    [HttpPost]
    public async Task<ActionResult<CreditCardClientResponse>> CreateCardAsync(CreditCardRequest createRequest)
    {
        _logger.LogInformation($"Creating card: {createRequest}");
        return await _creditCardService.CreateCreditCardAsync(createRequest);
    }

    [HttpPut("{cardId}")]
    public async Task<ActionResult<CreditCardClientResponse>> UpdateCardAsync(string cardId,
        CreditCardUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating card with id {cardId}");
        var result = await _creditCardService.UpdateCreditCardAsync(cardId, updateRequest);

        if (result == null) return NotFound();

        return Ok(result);
    }

    [HttpDelete("{cardId}")]
    public async Task<IActionResult> DeleteCardAsync(string cardId)
    {
        _logger.LogInformation($"Deleting card with id {cardId}");
        await _creditCardService.DeleteCreditCardAsync(cardId);
        return NoContent();
    }

}