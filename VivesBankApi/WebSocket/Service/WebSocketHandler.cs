using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using VivesBankApi.WebSocket.Model;

namespace VivesBankApi.WebSocket.Service;

public class WebSocketHandler : IWebsocketHandler
{
    private readonly ILogger _logger;
    private readonly List<System.Net.WebSockets.WebSocket> _sockets = new();
    private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _userSockets = new();

    public WebSocketHandler(ILogger<WebSocketHandler> logger)
    {
        _logger = logger;
    }

    // Este método se encarga de manejar las conexiones WebSocket entrantes
    public async Task HandleAsync(System.Net.WebSockets.WebSocket webSocket)
    {
        _logger.LogInformation("WebSocket connected from {0}", webSocket);
        _sockets.Add(webSocket);

        var buffer = new byte[1024 * 4]; // Buffer para leer los datos del WebSocket
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        // Mientras la conexión esté abierta, leemos los datos del WebSocket
        while (!result.CloseStatus.HasValue)
            // Convertimos los datos recibidos a texto
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        // Cerramos la conexión WebSocket y la eliminamos de la lista
        _sockets.Remove(webSocket);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }

    // Este método se encarga de enviar un mensaje a todos los clientes conectados
    public async Task NotifyAllAsync<T>(Notification<T> notification)
    {
        // Escribimos e ignoramos los valores nulos para evitar errores de serialización e idnetamos
        var jsonSettings = new JsonSerializerSettings { 
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };
        var json = JsonConvert.SerializeObject(notification, jsonSettings);
        _logger.LogInformation($"Notifying all clients: {json}");
        var buffer = Encoding.UTF8.GetBytes(json);
        // Enviamos el mensaje a todos los clientes conectados
        var tasks = _sockets.Select(socket =>
                socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                    CancellationToken.None))
            .ToArray();
        await Task.WhenAll(tasks); // Esperamos a que todos los envíos se completen
    }
    
    public async Task NotifyUserAsync<T>(string username, Notification<T> notification)
    {
        if (_userSockets.TryGetValue(username, out var socket))
        {
            var jsonSettings = new JsonSerializerSettings 
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(notification, jsonSettings);
            var buffer = Encoding.UTF8.GetBytes(json);
        
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogInformation($"Notification sent to {username}: {json}");
        }
        else
        {
            _logger.LogWarning($"User '{username}' not found.");
        }
    }
    
 

    public async Task HandleAuthenticatedAsync(System.Net.WebSockets.WebSocket webSocket, string username)
    {
        _logger.LogInformation($"WebSocket connected for user: {username}");
        _userSockets.AddOrUpdate(username, webSocket, (key, oldValue) => webSocket);
        _logger.LogInformation($"Number of connected users: {_userSockets.Count}");
    
        var buffer = new byte[1024 * 4];
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    
        while (!result.CloseStatus.HasValue)
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }
    
        _userSockets.TryRemove(username, out _);
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
    }


}