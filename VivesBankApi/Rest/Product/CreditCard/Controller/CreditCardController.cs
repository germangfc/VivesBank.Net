using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using VivesBankApi.Rest.Product.CreditCard.Dto;
using VivesBankApi.Rest.Product.CreditCard.Service;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;
using Newtonsoft.Json;

namespace VivesBankApi.Rest.Product.CreditCard.Controller;


/// <summary>
/// Controlador que gestiona las operaciones relacionadas con tarjetas de crédito.
/// Ofrece acciones tanto para usuarios administradores como para clientes,
/// permitiendo operaciones como obtener, crear, actualizar y eliminar tarjetas de crédito,
/// así como importar y exportar datos en formato JSON.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
[ApiController]
[Route("api/[controller]")]
public class CreditCardController : ControllerBase
{
    private readonly ICreditCardService _creditCardService;
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor que inyecta las dependencias del servicio de tarjetas de crédito y el logger.
    /// </summary>
    /// <param name="creditCardService">Servicio encargado de la lógica de negocio de las tarjetas de crédito.</param>
    /// <param name="logger">Instancia de logger para registrar las operaciones realizadas.</param>
    public CreditCardController(ICreditCardService creditCardService, ILogger<CreditCardController> logger)
    {
        _creditCardService = creditCardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tarjetas de crédito para administradores con soporte de paginación y filtrado.
    /// Requiere autenticación y autorización con la política "AdminPolicy".
    /// </summary>
    /// <param name="pageNumber">Número de página para la paginación.</param>
    /// <param name="pageSize">Cantidad de elementos por página.</param>
    /// <param name="fullName">Filtra por nombre completo.</param>
    /// <param name="isDeleted">Filtra por tarjetas eliminadas o no.</param>
    /// <param name="direction">Dirección de la ordenación (ascendente o descendente).</param>
    /// <returns>Lista de tarjetas de crédito administradas.</returns>
    [HttpGet]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<List<CreditCardAdminResponse>>> GetAllCardsAdminAsync([FromQuery] int pageNumber = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string fullName = "",
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string direction = "asc")
    {
        _logger.LogInformation("Getting all credit cards");
        var cards = await _creditCardService.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted,
            direction);
        return Ok(cards);
    }

    /// <summary>
    /// Obtiene una tarjeta de crédito específica por su identificador para administradores.
    /// Requiere autenticación y autorización con la política "AdminPolicy".
    /// </summary>
    /// <param name="cardId">Identificador único de la tarjeta.</param>
    /// <returns>Detalles de la tarjeta de crédito solicitada.</returns>
    [HttpGet("{cardId}")]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<CreditCardAdminResponse?>> GetCardByIdAdminAsync(string cardId)
    {
        _logger.LogInformation($"Getting card with id {cardId}");
        var card = await _creditCardService.GetCreditCardByIdAdminAsync(cardId);
        return Ok(card);
    }

    /// <summary>
    /// Obtiene las tarjetas de crédito asociadas al usuario actual.
    /// Requiere autenticación y autorización con la política "ClientPolicy".
    /// </summary>
    /// <returns>Lista de tarjetas de crédito del cliente.</returns>
    [HttpGet("me")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<List<CreditCardClientResponse>>> GetMyCardsAsync()
    {
        _logger.LogInformation("Getting my credit cards");
        var cards = await _creditCardService.GetMyCreditCardsAsync();
        return Ok(cards);
    }

    /// <summary>
    /// Crea una nueva tarjeta de crédito para el cliente.
    /// Requiere autenticación y autorización con la política "ClientPolicy".
    /// </summary>
    /// <param name="createRequest">Datos para crear la nueva tarjeta.</param>
    /// <returns>Detalles de la tarjeta de crédito creada.</returns>
    [HttpPost]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<CreditCardClientResponse>> CreateCardAsync([FromBody] CreditCardRequest createRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation($"Creating card: {createRequest}");
        var card = await _creditCardService.CreateCreditCardAsync(createRequest);
        return CreatedAtAction(nameof(GetCardByIdAdminAsync), new { cardId = card.Id }, card);
    }

    /// <summary>
    /// Actualiza una tarjeta de crédito existente.
    /// Requiere autenticación y autorización con la política "ClientPolicy".
    /// </summary>
    /// <param name="number">Número de la tarjeta a actualizar.</param>
    /// <param name="updateRequest">Nuevo estado de la tarjeta a actualizar.</param>
    /// <returns>Detalles de la tarjeta actualizada.</returns>
    [HttpPut("{number}")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<CreditCardClientResponse>> UpdateCardAsync(string number,
      [FromBody] CreditCardUpdateRequest updateRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _logger.LogInformation($"Updating card with id {number}");
        var card = await _creditCardService.UpdateCreditCardAsync(number, updateRequest);

        if (card == null)
        {
            return NotFound();
        }

        return Ok(card);
    }

    /// <summary>
    /// Elimina una tarjeta de crédito.
    /// Requiere autenticación y autorización con la política "ClientPolicy".
    /// </summary>
    /// <param name="cardnumber">Número de la tarjeta a eliminar.</param>
    /// <returns>Respuesta sin contenido si la eliminación fue exitosa.</returns>
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

    /// <summary>
    /// Importa tarjetas de crédito desde un archivo JSON.
    /// </summary>
    /// <param name="file">Archivo que contiene las tarjetas de crédito a importar.</param>
    /// <returns>Lista de tarjetas de crédito importadas.</returns>
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

            await _creditCardService.Import(file).ForEachAsync(creditCard => { creditCards.Add(creditCard); });

            return new OkObjectResult(creditCards);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error importing credit cards: {ex.Message}");
            return StatusCode(500, new { message = "Error importing credit cards", details = ex.Message });
        }
    }

    /// <summary>
    /// Exporta tarjetas de crédito a formato JSON o como archivo.
    /// </summary>
    /// <param name="asFile">Indica si los datos se deben devolver como archivo o como JSON.</param>
    /// <returns>Las tarjetas de crédito exportadas en formato JSON o como archivo.</returns>
    [HttpPost("export")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ExportCreditCardsToJson([FromQuery] bool asFile = true)
    {
        try
        {
            _logger.LogInformation($"asFile value: {asFile}");

            int pageNumber = 1;
            int pageSize = 100;
            string fullName = "";
            bool? isDeleted = false;
            string direction = "asc";

            var creditCardsAdminResponse = await _creditCardService.GetAllCreditCardAdminAsync(pageNumber, pageSize, fullName, isDeleted, direction);

            if (creditCardsAdminResponse == null || !creditCardsAdminResponse.Any())
            {
                _logger.LogWarning("No credit cards found.");
                return Ok(new { message = "No credit cards found" });
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

            if (!asFile)
            {
                _logger.LogInformation("Returning credit cards as JSON, not as file.");
                return Ok(creditCards); 
            }

            _logger.LogInformation("Returning credit cards as file.");
            var fileStream = await _creditCardService.Export(creditCards);

            if (fileStream == null)
            {
                _logger.LogError("Error generating the file.");
                return StatusCode(500, new { message = "Error generating the file" });
            }

            return File(fileStream, "application/json", "creditcards.json");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting credit cards: {ex.Message}");
            return StatusCode(500, new { message = "Error exporting credit cards", details = ex.Message });
        }
    }
}
