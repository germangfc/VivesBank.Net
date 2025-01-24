using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;

public class DomiciliacionRepository: IDomiciliacionRepository
{
    private readonly IMongoCollection<Domiciliacion> _collection;
    private readonly ILogger<DomiciliacionRepository> _logger;

    public DomiciliacionRepository(IOptions<MongoDatabaseConfig> mongoDatabaseSettings, ILogger<DomiciliacionRepository> logger)
    {
        var client = new MongoClient(mongoDatabaseSettings.Value.ConnectionString);
        
        var database = client.GetDatabase(mongoDatabaseSettings.Value.DatabaseName);
        
        _collection = database.GetCollection<Domiciliacion>(mongoDatabaseSettings.Value.DomiciliacionCollectionName);
        
        _logger = logger;
    }
    public async Task<List<Domiciliacion>> GetAllDomiciliacionesAsync()
    {
        _logger.LogInformation("Getting all domiciliaciones from the database.");
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Domiciliacion> GetDomiciliacionByIdAsync(String id)
    {
        _logger.LogInformation($"Getting domiciliacion with id {id} from the database.");
        return await _collection.Find(d => d.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Adding a new domiciliacion to the database: {domiciliacion}");
        await _collection.InsertOneAsync(domiciliacion);
        return domiciliacion;
    }

    public async Task<Domiciliacion> UpdateDomiciliacionAsync(String id, Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Updating domiciliacion with id {id} in the database.");
        var updateResult = await _collection.FindOneAndReplaceAsync(
            d => d.Id == id,
            domiciliacion,
            new FindOneAndReplaceOptions<Domiciliacion>{ ReturnDocument = ReturnDocument.After }
        );
        return updateResult;
    }

    public async Task<Domiciliacion> DeleteDomiciliacionAsync(String id)
    {
        _logger.LogInformation($"Deleting domiciliacion with id {id} from the database.");
        var deletedDomiciliacion = await _collection.FindOneAndDeleteAsync(d => d.Id == id);
        return deletedDomiciliacion;
    }

    public async Task<List<Domiciliacion>> GetDomiciliacionesActivasByClienteGiudAsync(string clienteGuid)
    {
        _logger.LogInformation($"Getting active domiciliaciones for client with guid {clienteGuid} from the database.");
        return await _collection.Find(d => d.ClienteGuid == clienteGuid && d.Activa).ToListAsync();
    }

    public async Task<List<Domiciliacion>> FindByClientGuid(string clientGuid)
    {
        _logger.LogInformation($"Finding domiciliaciones for client with guid {clientGuid}.");
        return await _collection.Find(d => d.ClienteGuid == clientGuid).ToListAsync();
    }
}