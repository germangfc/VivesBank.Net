using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Services;

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
        _logger.LogInformation($"Getting account with id {id}");
        return await _accountsService.GetAccountByIdAsync(id);
    }

    [HttpGet("iban/{iban}")]
    public async Task<ActionResult<AccountResponse>> GetAccountByIban(String iban)
    {
        _logger.LogInformation($"Getting account with IBAN {iban}");
        return await _accountsService.GetAccountByIbanAsync(iban);
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
        _logger.LogInformation($"Deleting account with id {id}");
        await _accountsService.DeleteAccountAsync(id);
        return NoContent();
    }
}
