using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.WebSocket.Controller;

/// <summary>
/// Controlador para gestionar las conexiones WebSocket en la API.
/// </summary>
/// <remarks>
/// Este controlador maneja las solicitudes WebSocket y las autentica antes de establecer la conexión.
/// </remarks>
[Route("api/[controller]")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly IWebsocketHandler _webSocketHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Constructor para inicializar el controlador de WebSocket.
    /// </summary>
    /// <param name="webSocketHandler">Servicio encargado de gestionar la lógica de WebSocket.</param>
    /// <param name="httpContextAccessor">Accesorio para acceder al contexto HTTP, útil para obtener información de usuario.</param>
    public WebSocketController(IWebsocketHandler webSocketHandler, IHttpContextAccessor httpContextAccessor)
    {
        _webSocketHandler = webSocketHandler;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Método para establecer una conexión WebSocket.
    /// </summary>
    /// <returns>Respuesta con el estado de la solicitud WebSocket.</returns>
    /// <remarks>
    /// Si la solicitud es una WebSocket, acepta la conexión y maneja la comunicación a través del servicio 
    /// <see cref="IWebsocketHandler"/>. En caso contrario, devuelve un error con código 400.
    /// </remarks>
    [HttpGet("/ws")]
    [Authorize]
    public async Task Get()
    {
        // Verifica si la solicitud es una conexión WebSocket
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Acepta la conexión WebSocket y maneja los mensajes/eventos dentro del handler
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var user = _httpContextAccessor.HttpContext!.User;
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _webSocketHandler.HandleAuthenticatedAsync(webSocket, id);
        }
        else
        {
            // Si no es una solicitud WebSocket, se devuelve un error 400
            HttpContext.Response.StatusCode = 400;
        }
    }
}
