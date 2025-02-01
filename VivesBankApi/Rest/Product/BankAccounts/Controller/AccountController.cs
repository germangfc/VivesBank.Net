using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;

namespace VivesBankApi.Rest.Product.BankAccounts.Controller;
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountsService  _accountsService;
    private ILogger _logger;
    public AccountController(IAccountsService accountsService, ILogger<AccountController> logger)
    {
        _accountsService = accountsService;
        _logger = logger;
    }
    
    
    [HttpGet]
    [Authorize("AdminPolicy")]
    public async Task<IActionResult> GetAllAccounts([FromQuery] int page = 0, [FromQuery] int size = 10, [FromQuery] string sortBy = "id", [FromQuery] string direction = "asc")
    {
        _logger.LogInformation("Getting all accounts with pagination");

        var response = await _accountsService.GetAccountsAsync(page, size, sortBy, direction);
        return Ok(response);
    }


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

    [HttpGet("me")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<List<AccountResponse>>> GetMyAccountsAsClientAsync()
    { 
        _logger.LogInformation("Getting my accounts as client");
        var accounts = await _accountsService.GetMyAccountsAsClientAsync();
        return Ok(accounts);
    }

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

    [HttpPost]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        _logger.LogInformation("Creating new account");
        var res =  await _accountsService.CreateAccountAsync(request);
        return Ok(res);
    }

    [HttpDelete("account/{iban}")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult> DeleteMyAccountAsClientAsync(String iban)
    {
        _logger.LogInformation($"Deleting account with IBAN {iban}");
        await _accountsService.DeleteMyAccountAsync(iban);
        return NoContent();
    }

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
