using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Service;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

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
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<List<CreditCardAdminResponse>>> GetAllCardsAdminAsync([FromQuery ]int pageNumber = 0, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string fullName = "",
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string direction = "asc")
    {
        _logger.LogInformation("Getting all credit cards");
        var cards = await _creditCardService.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);
        return Ok(cards);
    }
    
    [HttpGet("{cardId}")]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<CreditCardAdminResponse?>> GetCardByIdAdminAsync(string cardId)
    {
        _logger.LogInformation($"Getting card with id {cardId}");
        var card = await _creditCardService.GetCreditCardByIdAdminAsync(cardId);
        return Ok(card);
    }

    [HttpGet("me")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<List<CreditCardClientResponse>>> GetMyCardsAsync()
    {
        _logger.LogInformation("Getting my credit cards");
        var cards = await _creditCardService.GetMyCreditCardsAsync();
        return Ok(cards);
    }

    [HttpPost]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<CreditCardClientResponse>> CreateCardAsync(CreditCardRequest createRequest)
    {
        _logger.LogInformation($"Creating card: {createRequest}");
        var card = await _creditCardService.CreateCreditCardAsync(createRequest);
        return CreatedAtAction(nameof(GetCardByIdAdminAsync), new { cardId = card.Id }, card);
    }

    [HttpPut("{number}")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<CreditCardClientResponse>> UpdateCardAsync(string number,
        CreditCardUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating card with id {number}");
        var card = await _creditCardService.UpdateCreditCardAsync(number, updateRequest);
        
        if (card == null) 
        {
            return NotFound(); 
        }

        return Ok(card);
    }

    [HttpDelete("{cardnumber}")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> DeleteCardAsync(string cardnumber)
    {
        _logger.LogInformation($"Deleting card with id {cardnumber}");
        
        try
        {
            await _creditCardService.DeleteCreditCardAsync(cardnumber);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning($"Card with id {cardnumber} not found."); 
            return NotFound();
        }
    }
}