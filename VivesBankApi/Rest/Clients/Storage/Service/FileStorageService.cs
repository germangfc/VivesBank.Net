using VivesBankApi.Rest.Clients.storage.Config;
using Path = iText.Kernel.Geom.Path;

namespace VivesBankApi.Rest.Clients.Storage.Service;

/// <summary>
/// Servicio para almacenar archivos localmente en el sistema de archivos.
/// </summary>
/// <remarks>
/// Este servicio maneja la carga y almacenamiento de archivos en el sistema local.
/// Los archivos se guardan en una ubicación configurada, con un nombre de archivo basado en un nombre base y una marca de tiempo.
/// </remarks>
public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageConfig _config;
    private readonly ILogger<LocalFileStorageService> _logger;

    /// <summary>
    /// Constructor de la clase <see cref="LocalFileStorageService"/>.
    /// </summary>
    /// <param name="config">Configuración del almacenamiento de archivos.</param>
    /// <param name="logger">El logger para registrar información de las operaciones.</param>
    public LocalFileStorageService(FileStorageConfig config, ILogger<LocalFileStorageService> logger)
    {
        _config = config;
        _logger = logger;
    }

    
    /// <summary>
    /// Guarda un archivo en el sistema de archivos local.
    /// </summary>
    /// <param name="file">El archivo a guardar.</param>
    /// <param name="baseFileName">El nombre base que se usará para el archivo.</param>
    /// <returns>La ruta completa del archivo guardado en el sistema.</returns>
    /// <exception cref="Exception">Lanzada si el tamaño del archivo excede el máximo permitido o si el tipo de archivo no es permitido.</exception>
    public async Task<string> SaveFileAsync(IFormFile file, string baseFileName)
    {
        _logger.LogInformation($"Saving file: {file.FileName}");

        if (file.Length > _config.MaxFileSize)
        {
            throw new Exception("El tamaño del fichero excede del máximo permitido.");
        }

        var fileExtension = System.IO.Path.GetExtension(file.FileName);
        if (!_config.AllowedFileTypes.Contains(fileExtension))
        {
            throw new Exception("Tipo de fichero no permitido.");
        }

        var uploadPath = System.IO.Path.Combine(_config.UploadDirectory);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var fullFileName = $"{baseFileName}-{timestamp}{fileExtension}";
        var filePath = System.IO.Path.Combine(uploadPath, fullFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        _logger.LogInformation($"File saved: {filePath}");
        return filePath;
    }
}
