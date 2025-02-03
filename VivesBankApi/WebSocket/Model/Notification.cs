namespace VivesBankApi.WebSocket.Model;

/// <summary>
/// Representa una notificación genérica que contiene datos y el tipo de acción que ocurrió.
/// </summary>
/// <typeparam name="T">Tipo de datos asociados con la notificación.</typeparam>
public class Notification<T>
{
    /// <summary>
    /// Enumera los tipos de notificaciones posibles.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Notificación para ejecutar una acción.
        /// </summary>
        Execute,

        /// <summary>
        /// Notificación para crear un nuevo elemento.
        /// </summary>
        Create,

        /// <summary>
        /// Notificación para actualizar un elemento existente.
        /// </summary>
        Update,

        /// <summary>
        /// Notificación para eliminar un elemento.
        /// </summary>
        Delete
    }

    /// <summary>
    /// Los datos asociados con la notificación.
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// El tipo de la notificación que indica la acción realizada.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Fecha y hora en la que la notificación fue creada.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
