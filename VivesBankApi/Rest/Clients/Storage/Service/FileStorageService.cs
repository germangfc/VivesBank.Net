using VivesBankApi.Rest.Clients.storage.Config;
using Path = iText.Kernel.Geom.Path;

namespace VivesBankApi.Rest.Clients.Storage.Service;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageConfig _config;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(FileStorageConfig config, ILogger<LocalFileStorageService> logger)
    {
        _config = config;
        _logger = logger;
    }

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
