using System.IO.Compression;
using System.IO;
using System.Reactive.Linq;
using System.Text.Json;
using VivesBankApi.Backup;
using VivesBankApi.Backup.Exceptions;
using VivesBankApi.Backup.Service;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.Base.Service;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Product.Service;
using VivesBankApi.Rest.Users.Service;
using Path = System.IO.Path;

namespace VivesBankApi.Utils.Backup
{
    /// <summary>
    /// Servicio encargado de manejar las operaciones de respaldo como la exportación e importación de datos.
    /// @author Raul Fernandez, Samuel Cortes, Javier Hernandez, Alvaro Herrero, German, Tomas
    /// </summary>
    public class BackupService : IBackupService
    {
        // Nombre del directorio temporal usado durante las operaciones de respaldo/exportación
        private static readonly string TempDirName = "StorageServiceTemp";

        // Dependencias inyectadas a través del constructor
        private readonly ILogger<BackupService> _logger; // Logger para registrar eventos y errores

        private readonly IClientService
            _clientService; // Interfaz de servicio de clientes para interactuar con los datos de los clientes

        private readonly IUserService
            _userService; // Interfaz de servicio de usuarios para interactuar con los datos de los usuarios

        private readonly IProductService
            _productService; // Servicio de productos para interactuar con los datos de productos

        private readonly ICreditCardService
            _creditCardService; // Servicio de tarjetas de crédito para gestionar la información de tarjetas de crédito

        private readonly IAccountsService
            _bankAccountService; // Servicio de cuentas bancarias para gestionar la información de cuentas bancarias

        private readonly IMovimientoService
            _movementService; // Servicio de movimientos para acceder a los datos de transacciones

        /// <summary>
        /// Constructor que inyecta las dependencias necesarias para el funcionamiento del servicio.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos y errores</param>
        /// <param name="clientService">Servicio de clientes</param>
        /// <param name="userService">Servicio de usuarios</param>
        /// <param name="productService">Servicio de productos</param>
        /// <param name="creditCardService">Servicio de tarjetas de crédito</param>
        /// <param name="bankAccountService">Servicio de cuentas bancarias</param>
        /// <param name="movementService">Servicio de movimientos</param>
        public BackupService(
            ILogger<BackupService> logger,
            IClientService clientService,
            IUserService userService,
            IProductService productService,
            ICreditCardService creditCardService,
            IAccountsService bankAccountService,
            IMovimientoService movementService)
        {
            _logger = logger;
            _clientService = clientService;
            _userService = userService;
            _productService = productService;
            _creditCardService = creditCardService;
            _bankAccountService = bankAccountService;
            _movementService = movementService;
        }

        /// <summary>
        /// Exporta los datos a un archivo ZIP.
        /// </summary>
        /// <param name="zipRequest">Objeto que contiene la solicitud de exportación, incluyendo la ruta del archivo ZIP</param>
        /// <returns>Ruta del archivo ZIP exportado</returns>
        public async Task<string> ExportToZip(BackUpRequest zipRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(zipRequest.FilePath))
                {
                    throw new ArgumentException("La ruta del archivo no puede estar vacía.");
                }

                var directory = Path.GetDirectoryName(zipRequest.FilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var zip = ZipFile.Open(zipRequest.FilePath, ZipArchiveMode.Create))
                {
                    zip.CreateEntry("clients.json");
                    zip.CreateEntry("users.json");
                    zip.CreateEntry("products.json");
                    zip.CreateEntry("creditCards.json");
                    zip.CreateEntry("bankAccounts.json");
                    zip.CreateEntry("movements.json");
                }

                return zipRequest.FilePath;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new BackupException.BackupPermissionException("No se tienen permisos suficientes para crear el archivo ZIP.", ex);
            }
        }

        /// <summary>
        /// Importa los datos desde un archivo ZIP.
        /// </summary>
        /// <param name="zipFilePath">Objeto que contiene la ruta del archivo ZIP a importar</param>
        public async Task ImportFromZip(BackUpRequest zipFilePath)
        {
            _logger.LogInformation($"Importando datos desde ZIP: {zipFilePath.FilePath}");
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), TempDirName); // Ruta del directorio temporal

            try
            {
                // Verificar si el archivo ZIP existe
                if (!File.Exists(zipFilePath.FilePath))
                {
                    throw new BackupException.BackupFileNotFoundException(
                        $"El archivo {zipFilePath.FilePath} no fue encontrado.");
                }

                Directory.CreateDirectory(tempDir); // Crear el directorio temporal

                // Extraer el contenido del archivo ZIP
                ExtractZip(zipFilePath, tempDir);

                // Importar los archivos JSON desde el directorio temporal
                await ImportJsonFiles(tempDir);

                _logger.LogInformation("Datos importados exitosamente desde ZIP: {ZipFilePath}", zipFilePath);
            }
            catch (BackupException.BackupFileNotFoundException ex)
            {
                _logger.LogError(ex, "Error: archivo ZIP no encontrado.");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de permisos al acceder al archivo ZIP.");
                throw new BackupException.BackupPermissionException(
                    "No se tienen permisos suficientes para acceder al archivo ZIP.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error: el archivo JSON es corrupto o no válido.");
                throw new BackupException.BackupPermissionException(
                    "El archivo JSON dentro del ZIP es corrupto o no es válido.", ex);
            }
        }

        private async Task ImportJsonFiles(string tempDir)
        {
            foreach (var file in Directory.GetFiles(tempDir, "*.json"))
            {
                try
                {
                    var content = await File.ReadAllTextAsync(file);
                    var jsonObject = JsonSerializer.Deserialize<object>(content);

                    if (jsonObject == null)
                    {
                        throw new JsonException("El archivo JSON no es válido.");
                    }

                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"Error al procesar el archivo JSON: {file}");
                    throw new BackupException.BackupPermissionException(
                        $"El archivo JSON {file} es corrupto o no válido.", ex);
                }
            }
        }

        /// <summary>
        /// Extrae los archivos desde un archivo ZIP a un directorio temporal.
        /// </summary>
        /// <param name="zipFilePath">Ruta del archivo ZIP a extraer</param>
        /// <param name="tempDir">Directorio temporal donde se extraerán los archivos</param>
        private void ExtractZip(BackUpRequest zipFilePath, string tempDir)
        {
            try
            {
                using (var zip = ZipFile.OpenRead(zipFilePath.FilePath))
                {
                    foreach (var entry in zip.Entries)
                    {
                        var filePath = Path.Combine(tempDir, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                        entry.ExtractToFile(filePath, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer el archivo ZIP");
                throw new BackupException.BackupPermissionException("Hubo un error al extraer el archivo ZIP.", ex);
            }
        }

        /// <summary>
        /// Exporta los archivos JSON con los datos de clientes, usuarios, productos, etc.
        /// </summary>
        /// <param name="directoryPath">Ruta del directorio donde se guardarán los archivos JSON</param>
        private async Task ExportJsonFiles(string directoryPath)
        {
            try
            {
                _logger.LogInformation("Exportando archivos JSON a {DirectoryPath}", directoryPath);

                // Obtener todas las entidades necesarias
                var clientEntities = await _clientService.GetAll();
                var userEntities = await _userService.GetAll();
                var productEntities = await _productService.GetAll();
                var creditCardEntities = await _creditCardService.GetAll();
                var bankAccountEntities = await _bankAccountService.GetAll();
                var movementEntities = await _movementService.FindAllMovimientosAsync();

                // Crear y escribir archivos JSON
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "clients.json"),
                    JsonSerializer.Serialize(clientEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "users.json"),
                    JsonSerializer.Serialize(userEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "products.json"),
                    JsonSerializer.Serialize(productEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "creditCards.json"),
                    JsonSerializer.Serialize(creditCardEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "bankAccounts.json"),
                    JsonSerializer.Serialize(bankAccountEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "movements.json"),
                    JsonSerializer.Serialize(movementEntities));

                _logger.LogInformation("Archivos JSON exportados exitosamente: {Files}",
                    string.Join(", ", Directory.GetFiles(directoryPath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar los archivos JSON");
                throw new BackupException.BackupPermissionException("Hubo un error al exportar los archivos JSON.", ex);
            }
        }
    }

}
