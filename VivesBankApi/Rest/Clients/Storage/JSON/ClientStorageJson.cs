using Newtonsoft.Json;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Utils.GenericStorage.JSON;
using Path = System.IO.Path;

namespace VivesBankApi.Rest.Clients.Storage.JSON
{
    /// <summary>
    /// Clase para manejar el almacenamiento de datos de clientes en formato JSON.
    /// </summary>
    /// <remarks>
    /// Esta clase se encarga de serializar un objeto de cliente y almacenarlo en formato JSON en el sistema de archivos.
    /// El archivo JSON se guarda en un directorio predefinido y su nombre incluye el ID del cliente y una marca de tiempo.
    /// </remarks>
    public class ClientStorageJson : GenericStorageJson<Client>
    {
        private readonly ILogger<ClientStorageJson> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ClientStorageJson"/>.
        /// </summary>
        /// <param name="logger">El logger para registrar información de los procesos.</param>
        public ClientStorageJson(ILogger<ClientStorageJson> logger) : base(logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Exporta los datos de un cliente a un archivo JSON.
        /// </summary>
        /// <param name="client">El objeto de cliente cuyo dato será exportado a JSON.</param>
        /// <returns>Un <see cref="FileStream"/> del archivo JSON generado.</returns>
        /// <exception cref="IOException">Se lanza si hay problemas al escribir o acceder al archivo.</exception>
        /// <exception cref="UnauthorizedAccessException">Se lanza si no se tienen permisos para acceder o escribir en el directorio de destino.</exception>
        public async Task<FileStream> ExportOnlyMeData(Client client)
        {
            // Registrar el inicio del proceso de exportación
            _logger.LogInformation($"Exporting Client to a JSON file");

            // Serializar el objeto cliente a un formato JSON con una indentación legible
            var json = JsonConvert.SerializeObject(client, Formatting.Indented);

            // Establecer la ruta del directorio donde se guardará el archivo JSON
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "Json");

            // Si el directorio no existe, se crea
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Generar el nombre del archivo, que incluye el ID del cliente y la fecha/hora actuales
            var fileName = $"Client_WithId_{client.Id}_InSystem-{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(directoryPath, fileName);

            // Guardar el archivo JSON en el directorio especificado
            await File.WriteAllTextAsync(filePath, json);

            // Registrar el éxito del proceso de escritura
            _logger.LogInformation($"File written to: {filePath}");

            // Devolver un FileStream del archivo JSON recién creado para su descarga o procesamiento posterior
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}