namespace VivesBankApi.WebSocket.Model;

public class Notification<T>
{
    public enum NotificationType
    {
        Execute,
        Create,
        Update,
        Delete
    }

    public T Data { get; set; }
    public string Type { get; set; }
    public DateTime CreatedAt { get; set; }
}