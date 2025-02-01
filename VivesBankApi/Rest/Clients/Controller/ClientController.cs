using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Clients.Dto;
using VivesBankApi.Rest.Clients.Mappers;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Clients.storage.Config;
using VivesBankApi.Rest.Clients.storage.JSON;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Clients.Controller;
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IClientStorageJson _storage;
    private ILogger _logger;
    
    public ClientController(IClientService clientService, ILogger<ClientController> logger, IClientStorageJson storage)
    {
        _clientService = clientService;
        _storage = storage;
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
    
    [HttpGet("me/export")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> GetMeDataAsClient()
    {
        _logger.LogInformation("Exporting client data as a JSON file");
        try
        {
            var user = await _clientService.GettingMyClientData();
            var fileStream = await _storage.ExportOnlyMeData(user.FromDtoResponse());
            return File(fileStream, "application/json", "user.json");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error exporting client: {ex.Message}");
            return StatusCode(500, new { message = "Error exporting client", details = ex.Message });
        }
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
    public async Task<IActionResult> UpdateClientDniPhotoFtpAsync(string clientId, IFormFile file)
    {
        _logger.LogInformation($"Request to update DNI photo for client with ID: {clientId}");

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was provided or the file is empty.");
        }

        var fileName = await _clientService.UpdateClientPhotoDniAsync(clientId, file);
        return Ok(new { message = "DNI photo updated successfully", fileName });
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
    
    [HttpGet("ftp/{fileName}")]
    [Authorize("AdminPolicy")]
    public async Task<IActionResult> GetFileFromFtpAsync(string fileName)
    {
        _logger.LogInformation($"Request to get file with file name: {fileName}");

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest(new { message = "File name must be provided." });
        }

        var fileStream = await _clientService.GetFileFromFtpAsync(fileName);
        if (fileStream == null)
        {
            return NotFound(new { message = $"File with name {fileName} not found." });
        }

        var fileExtension = Path.GetExtension(fileName)?.ToLower();
        var mimeType = MimeTypes.GetMimeType(fileExtension);

        fileStream.Seek(0, SeekOrigin.Begin);

        return File(fileStream, mimeType, fileName);
    }
    
    
    [HttpGet("me-photo")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> GetMyProfilePhotoAsync()
    {
        _logger.LogInformation("Request to get my profile photo.");
        var fileStream = await _clientService.GettingMyProfilePhotoAsync();
        var mimeType = MimeTypes.GetMimeType(Path.GetExtension(fileStream.Name));
        return File(fileStream, mimeType, Path.GetFileName(fileStream.Name));
    }


    [HttpPatch("me-photo")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> UpdateMyProfilePhotoAsync(IFormFile file)
    {
        _logger.LogInformation("Request to update my profile photo.");

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file provided.");
            return BadRequest("No file provided.");
        }

        var newFileName = await _clientService.UpdateMyProfilePhotoAsync(file);
        _logger.LogInformation($"Profile photo updated successfully: {newFileName}");

        return Ok(new { message = "Profile photo updated successfully", fileName = newFileName });
    }
    
    [HttpPatch("me-dni-photo")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> UpdateMyDniPhotoAsync(IFormFile file)
    {
        _logger.LogInformation("Request to update my DNI photo.");

        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("No file provided.");
            return BadRequest("No file provided.");
        }

        var newFileName = await _clientService.UpdateMyPhotoDniAsync(file);
        _logger.LogInformation($"DNI photo updated successfully: {newFileName}");

        return Ok(new { message = "DNI photo updated successfully", fileName = newFileName });
    }


    [HttpGet("me-dni-photo")]
    [Authorize("ClientPolicy")]
    public async Task<IActionResult> GetMyDniPhotoFromFtpAsync()
    {
        _logger.LogInformation("Request to get my DNI photo from FTP.");

        var fileStream = await _clientService.GettingMyDniPhotoFromFtpAsync();

        var mimeType = MimeTypes.GetMimeType(Path.GetExtension(fileStream.Name));

        _logger.LogInformation($"Returning DNI photo from FTP: {Path.GetFileName(fileStream.Name)} with MIME type: {mimeType}");
        return File(fileStream, mimeType, Path.GetFileName(fileStream.Name));
    }
    
    
    
    
}