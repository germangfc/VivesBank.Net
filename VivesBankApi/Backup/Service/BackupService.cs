using System.IO.Compression;
using Newtonsoft.Json;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Product.Service;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Service;
using Path = System.IO.Path;

namespace VivesBankApi.Backup.Service;

public class BackupService : IBackupService
{
    private readonly ILogger _logger;
    
    private const string TempDirName = "StorageServiceTemp";
    private static readonly FileInfo DefaultBackupFile = new FileInfo("backup.zip");

    private readonly IClientService _clientService;
    private readonly IUserService _userService;
    private readonly IAccountsService _accountService;
    private readonly IProductService _productService;
    private readonly ICreditCardService _creditCardService;
    private readonly IMovimientoService _movimientosService;

    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountsRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IMovimientoRepository _movimientosRepository;

    public BackupService(
        IClientService clientService,
        IUserService userService,
        IAccountsService bankAccountService,
        IProductService productService,
        ICreditCardService creditCardService,
        IMovimientoService movementsService,
        IClientRepository clientRepository,
        IUserRepository userRepository,
        IAccountsRepository bankAccountRepository,
        IProductRepository productRepository,
        ICreditCardRepository creditCardRepository,
        IMovimientoRepository movementsRepository)
    {
        _clientService = clientService;
        _userService = userService;
        _accountService = bankAccountService;
        _productService = productService;
        _creditCardService = creditCardService;
        _movimientosService = movementsService;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _accountRepository = bankAccountRepository;
        _productRepository = productRepository;
        _creditCardRepository = creditCardRepository;
        _movimientosRepository = movementsRepository;
    }
    
    public async Task ImportFromZipAsync(FileInfo zipFile)
    {
        _logger.LogInformation("Importing data from ZIP: {ZipFileName}", zipFile.Name);

        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), TempDirName);
            Directory.CreateDirectory(tempDir);

            // Extract ZIP files
            using (ZipArchive archive = ZipFile.OpenRead(zipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    string filePath = Path.Combine(tempDir, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    entry.ExtractToFile(filePath, overwrite: true);
                }
            }

            // Import JSON files
            await _clientService.ImportJsonAsync(Path.Combine(tempDir, "clients.json"));
            await _userService.ImportJsonAsync(Path.Combine(tempDir, "users.json"));
            await _creditCardService.ImportJsonAsync(Path.Combine(tempDir, "creditCards.json"));
            await _accountService.ImportJsonAsync(Path.Combine(tempDir, "bankAccounts.json"));
            await _productService.ImportJsonAsync(Path.Combine(tempDir, "products.json"));
            await _movimientosService.ImportJsonAsync(Path.Combine(tempDir, "movements.json"));

            _logger.LogInformation("Data imported successfully from ZIP: {ZipFileName}", zipFile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data from ZIP");
            throw;
        }
    }
    

    public async Task exportToZip(FileInfo zipFile)
    {
        _logger.LogInformation("Exporting data to ZIP: {ZipFileName}", zipFile.Name);
    
        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), TempDirName);
            Directory.CreateDirectory(tempDir);

            await ExportJsonAsync(Path.Combine(tempDir, "clients.json"), await _clientRepository.GetAllAsync());
            await ExportJsonAsync(Path.Combine(tempDir, "users.json"), await _userRepository.GetAllAsync());
            await ExportJsonAsync(Path.Combine(tempDir, "creditCards.json"), await _creditCardRepository.GetAllAsync());
            await ExportJsonAsync(Path.Combine(tempDir, "bankAccounts.json"), await _accountRepository.GetAllAsync());
            await ExportJsonAsync(Path.Combine(tempDir, "products.json"), await _productRepository.GetAllAsync());
            await ExportJsonAsync(Path.Combine(tempDir, "movements.json"), await _movimientosRepository.GetAllMovimientosAsync());
            
            using (FileStream zipToCreate = new FileStream(zipFile.FullName, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in Directory.GetFiles(tempDir))
                {
                    string fileName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            _logger.LogInformation("Data exported successfully to ZIP: {ZipFileName}", zipFile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting data to ZIP");
            throw;
        }
    }
    
    private async Task ExportJsonAsync<T>(string filePath, IEnumerable<T> data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        await File.WriteAllTextAsync(filePath, json);
    }
}