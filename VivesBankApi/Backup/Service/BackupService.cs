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
    public class BackupService : IBackupService
    {
        private static readonly string TempDirName = "StorageServiceTemp";
        private readonly ILogger<BackupService> _logger;
        private readonly IClientService _clientService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ICreditCardService _creditCardService;
        private readonly IAccountsService _bankAccountService;
        private readonly IMovimientoService _movementService;

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

        public async Task ImportFromZip(BackUpRequest zipFilePath)
        {
            _logger.LogInformation($"Importing data from ZIP: {zipFilePath.FilePath}");
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), TempDirName);

            try
            {
                if (!File.Exists(zipFilePath.FilePath))
                {
                    throw new BackupException.BackupFileNotFoundException($"El archivo {zipFilePath.FilePath} no fue encontrado.");
                }

                Directory.CreateDirectory(tempDir);

                ExtractZip(zipFilePath, tempDir);

                await ImportJsonFiles(tempDir);

                _logger.LogInformation("Data imported successfully from ZIP: {ZipFilePath}", zipFilePath);
            }
            catch (BackupException.BackupFileNotFoundException ex)
            {
                _logger.LogError(ex, "Error: archivo ZIP no encontrado.");
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Error de permisos al acceder al archivo ZIP.");
                throw new BackupException.BackupPermissionException("No se tienen permisos suficientes para acceder al archivo ZIP.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error: el archivo JSON es corrupto o no válido.");
                throw new BackupException.BackupPermissionException("El archivo JSON dentro del ZIP es corrupto o no es válido.", ex);
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
                    throw new BackupException.BackupPermissionException($"El archivo JSON {file} es corrupto o no válido.", ex);
                }
            }
        }


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
                _logger.LogError(ex, "Error extracting ZIP file");
                throw new BackupException.BackupPermissionException("Hubo un error al extraer el archivo ZIP.", ex);
            }
        }
        
        
    }
}
