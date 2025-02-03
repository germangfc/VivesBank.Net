using VivesBankApi.WebSocket.Model;

namespace VivesBankApi.WebSocket.Service;

/// <summary>
/// Interface that defines the necessary methods to handle WebSocket connections and notifications.
/// </summary>
public interface IWebsocketHandler
{
    /// <summary>
    /// Handles an incoming WebSocket connection.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection to be handled.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAsync(System.Net.WebSockets.WebSocket webSocket);

    /// <summary>
    /// Sends a notification to a specific user via WebSocket.
    /// </summary>
    /// <typeparam name="T">The type of the notification data.</typeparam>
    /// <param name="username">The username of the user to notify.</param>
    /// <param name="notification">The notification to send.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyUserAsync<T>(string username, Notification<T> notification);

    /// <summary>
    /// Sends a notification to all users connected via WebSocket.
    /// </summary>
    /// <typeparam name="T">The type of the notification data.</typeparam>
    /// <param name="notification">The notification to send to all users.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task NotifyAllAsync<T>(Notification<T> notification);

    /// <summary>
    /// Handles a WebSocket connection for an authenticated user.
    /// </summary>
    /// <param name="webSocket">The WebSocket connection of the authenticated user.</param>
    /// <param name="username">The username of the authenticated user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HandleAuthenticatedAsync(System.Net.WebSockets.WebSocket webSocket, string username);
}
