using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
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
    public async Task<IActionResult> GetAllAccounts([FromQuery] int page = 0, [FromQuery] int size = 10, [FromQuery] string sortBy = "id", [FromQuery] string direction = "asc")
    {
        _logger.LogInformation("Getting all accounts with pagination");

        var response = await _accountsService.GetAccountsAsync(page, size, sortBy, direction);
        return Ok(response);
    }


    [HttpGet("{id}")]
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

    [HttpGet("iban/{iban}")]
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
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        _logger.LogInformation("Creating new account");
        return await _accountsService.CreateAccountAsync(request);
    }

    [HttpDelete("{id}")]
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
}
