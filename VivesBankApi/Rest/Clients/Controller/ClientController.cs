using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Service;

namespace VivesBankApi.Rest.Clients.Controller;
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private ILogger _logger;
    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all clients");
        var response = await _clientService.GetAllAsync();
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientResponse>> GetById(string id)
    {
        _logger.LogInformation($"Getting client with id {id}");
        return await _clientService.GetClientByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponse>> CreateClient([FromBody] ClientRequest request)
    {
        _logger.LogInformation("Creating new client");
        var client =  await _clientService.CreateClientAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientResponse>> UpdateClient(string id, ClientUpdateRequest request)
    {
        _logger.LogInformation($"Updating client with id {id}");
        var client = await _clientService.UpdateClientAsync(id, request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }
    
    [HttpDelete("{id}")]
    public async Task DeleteClient(string id)
    {
        _logger.LogInformation($"Deleting client with id {id}");
        await _clientService.LogicDeleteClientAsync(id);
    }
    
}