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
            _logger.LogInformation("Exporting data to ZIP: {ZipFilePath}", zipRequest.FilePath);

            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), TempDirName);
            var zipFilePath = zipRequest.FilePath;

            try
            {
                // Crear directorio temporal si no existe
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                // Exportar datos en formato JSON
                await ExportJsonFiles(tempDir);

                // Si el usuario no proporciona una ruta, guardar en la carpeta de backups
                if (string.IsNullOrWhiteSpace(zipFilePath))
                {
                    var backupFolder = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
                    Directory.CreateDirectory(backupFolder); // Asegurar que exista la carpeta
                    zipFilePath = Path.Combine(backupFolder, $"Backup_{DateTime.Now:yyyyMMddHHmmss}.zip");
                }

                // Crear ZIP con los JSON exportados
                using (var zip = new ZipArchive(File.Open(zipFilePath, FileMode.Create), ZipArchiveMode.Create))
                {
                    foreach (var filePath in Directory.GetFiles(tempDir))
                    {
                        zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                    }
                }

                _logger.LogInformation("Data exported successfully to ZIP: {ZipFilePath}", zipFilePath);
                return zipFilePath; // Devolver la ruta del archivo ZIP generado
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting data to ZIP");
                throw new BackupException.BackupPermissionException("Error al exportar los datos. Verifique permisos o el directorio.", ex);
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
                    throw new BackupException.BackupFileNotFoundException($"El archivo {zipFilePath} no fue encontrado.");
                }

                Directory.CreateDirectory(tempDir);

                ExtractZip(zipFilePath, tempDir);

                await ImportJsonFiles(tempDir);

                _logger.LogInformation("Data imported successfully from ZIP: {ZipFilePath}", zipFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing data from ZIP");
                throw new BackupException.BackupPermissionException("Hubo un error al intentar importar los datos. Verifique el archivo ZIP o los permisos.", ex);
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

        private async Task ExportJsonFiles(string directoryPath)
        {
            try
            {
                _logger.LogInformation("Exporting JSON files to {DirectoryPath}", directoryPath);

                var clientEntities = await _clientService.GetAll();
                var userEntities = await _userService.GetAll();
                var productEntities = await _productService.GetAll();
                var creditCardEntities = await _creditCardService.GetAll();
                var bankAccountEntities = await _bankAccountService.GetAll();
                var movementEntities = await _movementService.FindAllMovimientosAsync();

                // Crear y escribir archivos JSON en `directoryPath`
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "clients.json"), JsonSerializer.Serialize(clientEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "users.json"), JsonSerializer.Serialize(userEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "products.json"), JsonSerializer.Serialize(productEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "creditCards.json"), JsonSerializer.Serialize(creditCardEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "bankAccounts.json"), JsonSerializer.Serialize(bankAccountEntities));
                await File.WriteAllTextAsync(Path.Combine(directoryPath, "movements.json"), JsonSerializer.Serialize(movementEntities));

                _logger.LogInformation("Successfully exported JSON files: {Files}", string.Join(", ", Directory.GetFiles(directoryPath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting JSON files");
                throw new BackupException.BackupPermissionException("Hubo un error al exportar los archivos JSON.", ex);
            }
        }


        private async Task ImportJsonFiles(string directoryPath)
        {
            try
            {
                _logger.LogInformation("Importing JSON files from {DirectoryPath}", directoryPath);

                var clientEntities = await _clientService.ImportFromFile(Path.Combine(directoryPath, "clients.json"));
                var userEntities = await _userService.ImportFromFile(Path.Combine(directoryPath, "users.json"));
                var creditCardEntities = await _creditCardService.ImportFromFile(Path.Combine(directoryPath, "creditCards.json"));
                var bankAccountEntities = await _bankAccountService.ImportFromFile(Path.Combine(directoryPath, "bankAccounts.json"));
                var productEntities = await _productService.ImportFromFile(Path.Combine(directoryPath, "products.json"));
                var movementEntities = await _movementService.ImportFromFile(Path.Combine(directoryPath, "movements.json"));

                _logger.LogInformation("JSON import completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing JSON files");
                throw new BackupException.BackupPermissionException("Hubo un error al importar los archivos JSON.", ex);
            }
        }
    }
}
