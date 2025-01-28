using VivesBankApi.WebSocket.Model;

namespace VivesBankApi.WebSocket.Service;

public interface IWebsocketHandler
{
    Task HandleAsync(System.Net.WebSockets.WebSocket webSocket);
    Task NotifyUserAsync<T>(string username, Notification<T> notification);
    Task NotifyAllAsync<T>(Notification<T> notification);
    Task HandleAuthenticatedAsync(System.Net.WebSockets.WebSocket webSocket, string username);
}