using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;

namespace VivesBankApi.Rest.Product.BankAccounts.Controller;



/// <summary>
/// Controlador para la gestión de cuentas bancarias.
/// Proporciona endpoints para la creación, eliminación, obtención y exportación de cuentas.
/// </summary>
/// <remarks>
/// Esta clase se encarga de manejar las solicitudes HTTP relacionadas con las cuentas bancarias de los usuarios y administradores.
/// Los métodos permiten obtener las cuentas, crear nuevas, eliminarlas, exportarlas e importarlas.
/// </remarks>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountsService  _accountsService;
    private ILogger _logger;

    /// <summary>
    /// Constructor para inyectar dependencias.
    /// </summary>
    /// <param name="accountsService">Servicio que maneja las operaciones relacionadas con cuentas.</param>
    /// <param name="logger">Instancia para registrar logs.</param>
    public AccountController(IAccountsService accountsService, ILogger<AccountController> logger)
    {
        _accountsService = accountsService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las cuentas con paginación.
    /// </summary>
    /// <param name="page">Número de página para paginación.</param>
    /// <param name="size">Número de elementos por página.</param>
    /// <param name="sortBy">Campo para ordenar las cuentas.</param>
    /// <param name="direction">Dirección de ordenación (asc/desc).</param>
    /// <returns>Lista paginada de cuentas.</returns>
    [HttpGet]
    [Authorize("AdminPolicy")]
    public async Task<IActionResult> GetAllAccounts([FromQuery] int page = 0, [FromQuery] int size = 10, [FromQuery] string sortBy = "id", [FromQuery] string direction = "asc")
    {
        _logger.LogInformation("Getting all accounts with pagination");
        var response = await _accountsService.GetAccountsAsync(page, size, sortBy, direction);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene una cuenta por su ID.
    /// </summary>
    /// <param name="id">ID de la cuenta.</param>
    /// <returns>Cuenta bancaria encontrada o 404 si no existe.</returns>
    [HttpGet("{id}")]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<AccountResponse>> GetAccountById(string id)
    {
        try
        {
            _logger.LogInformation($"Getting account with id {id}");
            var account = await _accountsService.GetAccountByIdAsync(id);
            if(account == null)
                return NotFound();
            return Ok(account);
        }
        catch (AccountsExceptions.AccountNotFoundException e)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Obtiene todas las cuentas asociadas al cliente autenticado.
    /// </summary>
    /// <returns>Lista de cuentas del cliente.</returns>
    [HttpGet("me")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<List<AccountResponse>>> GetMyAccountsAsClientAsync()
    { 
        _logger.LogInformation("Getting my accounts as client");
        var accounts = await _accountsService.GetMyAccountsAsClientAsync();
        return Ok(accounts);
    }

    /// <summary>
    /// Obtiene una cuenta por su IBAN.
    /// </summary>
    /// <param name="iban">IBAN de la cuenta.</param>
    /// <returns>Cuenta bancaria encontrada o 404 si no existe.</returns>
    [HttpGet("iban/{iban}")]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<AccountResponse>> GetAccountByIban(String iban)
    {
        try
        {
            _logger.LogInformation($"Getting account with IBAN {iban}");
            var account = await _accountsService.GetAccountByIbanAsync(iban);
            return Ok(account);
        }
        catch (AccountsExceptions.AccountNotFoundByIban e)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Crea una nueva cuenta para un cliente.
    /// </summary>
    /// <param name="request">Datos necesarios para crear una cuenta.</param>
    /// <returns>La cuenta creada.</returns>
    [HttpPost]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        _logger.LogInformation("Creating new account");
        var res = await _accountsService.CreateAccountAsync(request);
        return Ok(res);
    }

    /// <summary>
    /// Elimina la cuenta de un cliente autenticado mediante su IBAN.
    /// </summary>
    /// <param name="iban">IBAN de la cuenta a eliminar.</param>
    /// <returns>Estado de la operación.</returns>
    [HttpDelete("account/{iban}")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult> DeleteMyAccountAsClientAsync(String iban)
    {
        _logger.LogInformation($"Deleting account with IBAN {iban}");
        await _accountsService.DeleteMyAccountAsync(iban);
        return NoContent();
    }

    /// <summary>
    /// Elimina una cuenta por su ID.
    /// </summary>
    /// <param name="id">ID de la cuenta a eliminar.</param>
    /// <returns>Estado de la operación.</returns>
    [HttpDelete("{id}")]
    [Authorize("AdminPolicy")]
    public async Task<ActionResult> DeleteAccount(string id)
    {
        try
        {
            _logger.LogInformation($"Deleting account with id {id}");
            await _accountsService.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (AccountsExceptions.AccountNotFoundException e)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Importa cuentas desde un archivo JSON.
    /// </summary>
    /// <param name="file">Archivo JSON con las cuentas a importar.</param>
    /// <returns>Lista de cuentas importadas.</returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportAccountsFromJson([Required] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            _logger.LogError("No file uploaded or file is empty.");
            return BadRequest("No file uploaded.");
        }

        try
        {
            var accounts = new List<Account>();
            _logger.LogInformation("Importing accounts from the uploaded file.");

            var observable = _accountsService.Import(file);
            var tcs = new TaskCompletionSource<bool>();

            observable.Subscribe(
                account =>
                {
                    accounts.Add(account);
                },
                ex =>
                {
                    _logger.LogError($"Error during import: {ex.Message}");
                    tcs.SetException(ex);
                },
                () =>
                {
                    _logger.LogInformation($"Successfully imported {accounts.Count} accounts.");
                    tcs.SetResult(true);
                });

            await tcs.Task;

            return Ok(accounts);
        }
        catch (FormatException ex)
        {
            _logger.LogError($"Invalid file format: {ex.Message}");
            return BadRequest(new { message = "Invalid file format", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error importing accounts: {ex.Message}");
            return StatusCode(500, new { message = "Error importing accounts", details = ex.Message });
        }
    }

    /// <summary>
    /// Exporta todas las cuentas a un archivo JSON.
    /// </summary>
    /// <returns>Archivo JSON con las cuentas exportadas.</returns>
    [HttpPost("export")]
    public async Task<IActionResult> ExportAccountsToJson()
    {
        try
        {
            var accountResponses = await _accountsService.GetAccountsAsync();

            if (accountResponses == null || !accountResponses.Content.Any())
            {
                return NoContent();
            }

            var accounts = accountResponses.Content.Select(ar => new Account
            {
                Id = ar.Id,
                ProductId = ar.productID,
                ClientId = ar.clientID,
                IBAN = ar.IBAN,
                AccountType = ar.AccountType,
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                TarjetaId = null
            }).ToList();

            var fileStream = await _accountsService.Export(accounts);

            return File(fileStream, "application/json", "accounts.json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error while exporting accounts", details = ex.Message });
        }
    }
}

