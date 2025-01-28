using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Clients.storage;
using VivesBankApi.Rest.Clients.storage.Config;
using Path = System.IO.Path;

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
    [Authorize("AdminPolicy")]
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
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<ClientResponse>> GetById(string id)
    {
        _logger.LogInformation($"Getting client with id {id}");
        return await _clientService.GetClientByIdAsync(id);
    }

    [HttpGet("me")] 
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<ClientResponse>> GetMyClientData()
    {
        _logger.LogInformation("Getting my client data");
        return await _clientService.GettingMyClientData();
    }

    [HttpPost("toclient")]
    [Authorize("UserPolicy")]
    public async Task<IActionResult> CreateClientAsUser([FromBody] ClientRequest request)
    {
        _logger.LogInformation("Creating new client");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var client = await _clientService.CreateClientAsync(request);
        return Ok(new { client });
    }
    
    [HttpPut("{id}")]
    [Authorize("AdminPolicy")]
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
    
    [HttpPut("me")]
    [Authorize("ClientPolicy")]
    public async Task<ActionResult<ClientResponse>> UpdateMeAsClient(ClientUpdateRequest request)
    {
        _logger.LogInformation($"Updating client registered on the system");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var client = await _clientService.UpdateMeAsync(request);
        return Ok(client);
    }
    
    
    [HttpDelete("{id}")]
    [Authorize("AdminPolicy")]
    public async Task DeleteClient(string id)
    {
        _logger.LogInformation($"Deleting client with id {id}");
        await _clientService.LogicDeleteClientAsync(id);
    }
    
    [HttpDelete("baja")]
    [Authorize("ClientPolicy")]
    public async Task DeleteMeClient()
    {
        _logger.LogInformation($"Deleting client registered on the system");
        await _clientService.DeleteMe();
    }
    
    
    [HttpPatch("{clientId}/dni")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> UpdateClientDniPhotoAsync(string clientId, IFormFile file)
    {
        _logger.LogInformation($"Request to update DNI photo for client with ID: {clientId}");
        
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was provided or the file is empty.");
        }

        var fileName = await _clientService.UpdateClientDniPhotoAsync(clientId, file);
        return Ok(new { message = "DNI photo updated successfully", fileName });
    }
    
    [HttpPatch("{clientId}/profile")]
    public async Task<IActionResult> UpdateClientPhotoAsync(string clientId, IFormFile file)
    {
        _logger.LogInformation($"Request to update profile photo for client with ID: {clientId}");

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was provided or the file is empty.");
        }

        var fileName = await _clientService.UpdateClientPhotoAsync(clientId, file);
        return Ok(new { message = "Profile photo updated successfully", fileName });
    }

    [HttpGet("photo/{fileName}")]
    [Authorize("AdminPolicy")]
    public async Task<IActionResult> GetPhotoByFileNameAsync(string fileName)
    {
        _logger.LogInformation($"Request to get photo with file name: {fileName}");

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "File name must be provided." });
        }

        var fileStream = await _clientService.GetFileAsync(fileName);
        if (fileStream == null)
        {
            return NotFound(new { message = $"File with name {fileName} not found." });
        }

        var fileExtension = Path.GetExtension(fileName);
        var mimeType = MimeTypes.GetMimeType(fileExtension);

        return File(fileStream, mimeType, fileName);
    }
    
}