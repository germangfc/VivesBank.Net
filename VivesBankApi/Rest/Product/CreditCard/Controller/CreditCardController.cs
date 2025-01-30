using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using AutoMapper;
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
    
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportCreditCardsFromJson([Required] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var creditCards = new List<Models.CreditCard>();

            await _creditCardService.Import(file).ForEachAsync(creditCard =>
            {
                creditCards.Add(creditCard);
            });

            return Ok(creditCards); 
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error importing credit cards: {ex.Message}");
            return StatusCode(500, new { message = "Error importing credit cards", details = ex.Message });
        }
    }
    
    [HttpPost("export")]
    public async Task<IActionResult> ExportCreditCardsToJson()
    {
        try
        {
            var creditCardsAdminResponse = await _creditCardService.GetAllCreditCardAdminAsync();

            if (creditCardsAdminResponse == null || !creditCardsAdminResponse.Any())
            {
                return NotFound(new { message = "No credit cards found to export." });
            }

            var creditCards = creditCardsAdminResponse.Select(card => new Models.CreditCard
            {
                Id = card.Id,
                AccountId = card.AccountId,
                CardNumber = card.CardNumber,
                ExpirationDate = DateOnly.Parse(card.ExpirationDate), 
                CreatedAt = card.CreatedAt,
                UpdatedAt = card.UpdatedAt,
                IsDeleted = false 
            }).ToList();



            var fileStream = await _creditCardService.Export(creditCards);

            return File(fileStream, "application/json", "creditcards.json");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting credit cards: {ex.Message}");
            return StatusCode(500, new { message = "Error exporting credit cards", details = ex.Message });
        }
    }
}