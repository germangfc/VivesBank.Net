using System.IO.Compression;
using System.Reactive.Linq;
using Newtonsoft.Json;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Repositories;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Models;
using VivesBankApi.Rest.Product.BankAccounts.Repositories;
using VivesBankApi.Rest.Product.Base.Models;
using VivesBankApi.Rest.Product.CreditCard.Models;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Utils.GenericStorage.JSON;
using Path = System.IO.Path;

namespace VivesBankApi.Backup.Service;

public class BackupService : IBackupService
{
    private readonly ILogger _logger;
    
    
    private const string TempDirName = "StorageServiceTemp";
    private static readonly FileInfo DefaultBackupFile = new FileInfo("backup.zip");

    private readonly IGenericStorageJson<Client> _genericStorageClient;
    private readonly IGenericStorageJson<User> _genericStorageUser;
    private readonly IGenericStorageJson<CreditCard> _genericStorageCreditCard;
    private readonly IGenericStorageJson<Account> _genericStorageBankAccount;
    private readonly IGenericStorageJson<Product> _genericStorageProduct;
    private readonly IGenericStorageJson<Movimiento> _genericStorageMovement;

    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAccountsRepository _accountRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICreditCardRepository _creditCardRepository;
    private readonly IMovimientoRepository _movimientosRepository;

    public BackupService(
        IGenericStorageJson<Client> genericStorageClient,
        IGenericStorageJson<User> genericStorageUser,
        IGenericStorageJson<CreditCard> genericStorageCreditCard,
        IGenericStorageJson<Account> genericStorageBankAccount,
        IGenericStorageJson<Product> genericStorageProduct,
        IGenericStorageJson<Movimiento> genericStorageMovement,
        
        IClientRepository clientRepository,
        IUserRepository userRepository,
        IAccountsRepository accountRepository,
        IProductRepository productRepository,
        ICreditCardRepository creditCardRepository,
        IMovimientoRepository movimientosRepository)
    {
        _genericStorageClient = genericStorageClient;
        _genericStorageUser = genericStorageUser;
        _genericStorageCreditCard = genericStorageCreditCard;
        _genericStorageBankAccount = genericStorageBankAccount;
        _genericStorageProduct = genericStorageProduct;
        _genericStorageMovement = genericStorageMovement;

        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _productRepository = productRepository;
        _creditCardRepository = creditCardRepository;
        _movimientosRepository = movimientosRepository;
    }

    
    public async Task ImportFromZipAsync(FileInfo zipFile)
    {
        _logger.LogInformation("Importing data from ZIP: {ZipFileName}", zipFile.Name);

        try
        {
            string tempDir = Path.Combine(Path.GetTempPath(), TempDirName);
            Directory.CreateDirectory(tempDir);

            using (ZipArchive archive = ZipFile.OpenRead(zipFile.FullName))
            {
                foreach (var entry in archive.Entries)
                {
                    string filePath = Path.Combine(tempDir, Path.GetFileName(entry.FullName));

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                
                    entry.ExtractToFile(filePath, overwrite: true);
                }
            }

            // Importar archivos JSON de manera genérica
            await ImportJsonAsync<Client>(_genericStorageClient, Path.Combine(tempDir, "clients.json"));
            await ImportJsonAsync<User>(_genericStorageUser, Path.Combine(tempDir, "users.json"));
            await ImportJsonAsync<CreditCard>(_genericStorageCreditCard, Path.Combine(tempDir, "creditCards.json"));
            await ImportJsonAsync<Account>(_genericStorageBankAccount, Path.Combine(tempDir, "bankAccounts.json"));
            await ImportJsonAsync<Product>(_genericStorageProduct, Path.Combine(tempDir, "products.json"));
            await ImportJsonAsync<Movimiento>(_genericStorageMovement, Path.Combine(tempDir, "movements.json"));

            _logger.LogInformation("Data imported successfully from ZIP: {ZipFileName}", zipFile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data from ZIP");
            throw;
        }
    }

    


    public async Task ExportToZipAsync(FileInfo zipFile)
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