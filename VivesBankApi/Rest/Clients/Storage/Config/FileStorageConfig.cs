namespace VivesBankApi.Rest.Clients.storage.Config;

public class FileStorageConfig
{
    // Directorio donde se almacenarán los archivos subidos
    public string UploadDirectory { get; set; } = "uploads";

    // Tamaño máximo permitido para los archivos (en bytes). Por defecto 10 MB
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;

    // Tipos de archivo permitidos para la carga
    public List<string> AllowedFileTypes { get; set; } = new List<string> { ".jpg", ".jpeg", ".png" };

    // Indica si se deben eliminar todos los archivos
    public bool RemoveAll { get; set; } = false;

    // Propiedad adicional que no se está usando en el código proporcionado
    public string SomeProperty { get; set; }
}
