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
        return Ok(card);
    }

    [HttpPost]
    public async Task<ActionResult<CreditCardClientResponse>> CreateCardAsync(CreditCardRequest createRequest)
    {
        _logger.LogInformation($"Creating card: {createRequest}");
        var card = await _creditCardService.CreateCreditCardAsync(createRequest);
        return CreatedAtAction(nameof(GetCardByIdAdminAsync), new { cardId = card.Id }, card);
    }

    [HttpPut("{cardId}")]
    public async Task<ActionResult<CreditCardClientResponse>> UpdateCardAsync(string cardId,
        CreditCardUpdateRequest updateRequest)
    {
        _logger.LogInformation($"Updating card with id {cardId}");
        var card = await _creditCardService.UpdateCreditCardAsync(cardId, updateRequest);
        
        if (card == null) 
        {
            return NotFound(); 
        }
        
        return CreatedAtAction(nameof(GetCardByIdAdminAsync), new { cardId = card.Id }, card);
    }

    [HttpDelete("{cardId}")]
    public async Task<IActionResult> DeleteCardAsync(string cardId)
    {
        _logger.LogInformation($"Deleting card with id {cardId}");
        
        try
        {
            await _creditCardService.DeleteCreditCardAsync(cardId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning($"Card with id {cardId} not found."); 
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