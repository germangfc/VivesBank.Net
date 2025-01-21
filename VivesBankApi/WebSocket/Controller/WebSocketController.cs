using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.WebSocket.Controller;

[Route("api/[controller]")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketHandler _webSocketHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebSocketController(WebSocketHandler webSocketHandler, IHttpContextAccessor httpContextAccessor)
    {
        _webSocketHandler = webSocketHandler;
        _httpContextAccessor = httpContextAccessor;
    }
    
    [HttpGet("/ws")]
    [Authorize]
    public async Task Get()
    {
        // Handle WebSocket connections
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Accept the WebSocket connection and handle messages/events within the WebSocketHandler class.
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var user = _httpContextAccessor.HttpContext!.User;
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _webSocketHandler.HandleAuthenticatedAsync(webSocket, id);
        }
        else
        {
            // Bad request response
            HttpContext.Response.StatusCode = 400;
        }
    }
}