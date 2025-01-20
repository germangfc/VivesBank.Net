using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;
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
    public async Task<ActionResult<PageResponse<ClientResponse>>> GetAllUsersAsync(
        [FromQuery ]int pageNumber = 0, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string fullName = "",
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string direction = "asc")
    {
        PagedList<ClientResponse> pagedList =  await _clientService.GetAllClientsAsync(
            pageNumber, pageSize, fullName, isDeleted, direction
        );

        return new PageResponse<ClientResponse>
        {
            Content = pagedList.ToList(),
            TotalPages = pagedList.PageCount,
            TotalElements = pagedList.TotalCount,
            PageSize = pagedList.PageSize,
            PageNumber = pagedList.PageNumber,
            TotalPageElements = pagedList.Count,
            Empty = pagedList.Count == 0,
            First = pagedList.IsFirstPage,
            Last = pagedList.IsLastPage,
            SortBy = "fullName",
            Direction = direction
        };
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
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var client =  await _clientService.CreateClientAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientResponse>> UpdateClient(string id, ClientUpdateRequest request)
    {
        _logger.LogInformation($"Updating client with id {id}");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var client = await _clientService.UpdateClientAsync(id, request);
        return Ok(client);
    }
    
    [HttpDelete("{id}")]
    public async Task DeleteClient(string id)
    {
        _logger.LogInformation($"Deleting client with id {id}");
        await _clientService.LogicDeleteClientAsync(id);
    }
    
}