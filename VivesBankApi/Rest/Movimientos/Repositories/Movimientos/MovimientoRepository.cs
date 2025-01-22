using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Movimientos;

public class MovimientoRepository : IMovimientoRepository
{
    private readonly IMongoCollection<Movimiento> _collection;
    private readonly ILogger<MovimientoRepository> _logger;
    public MovimientoRepository(IOptions<MongoDatabaseConfig> mongoDatabaseSettings, ILogger<MovimientoRepository> logger)
    {
        var client = new MongoClient(mongoDatabaseSettings.Value.ConnectionString);
        
        var database = client.GetDatabase(mongoDatabaseSettings.Value.DatabaseName);
        
        _collection = database.GetCollection<Movimiento>(mongoDatabaseSettings.Value.MovimientosCollectionName);

        _logger = logger;
    }
    public async Task<List<Movimiento>> GetAllMovimientosAsync()
    {
        _logger.LogInformation("Getting all movimientos from the database.");
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<Movimiento> GetMovimientoByIdAsync(String id)
    {
        _logger.LogInformation($"Getting movimiento with id: {id} from the database.");
        return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Movimiento> GetMovimientoByGuidAsync(string guid)
    {
        _logger.LogInformation($"Getting movimiento with guid: {guid} from the database.");
        return await _collection.Find(m => m.Guid == guid).FirstOrDefaultAsync();
    }

    public async Task<List<Movimiento>> GetMovimientosByClientAsync(string clienteId)
    {
        _logger.LogInformation($"Getting movimientos for client with id: {clienteId} from the database.");
        return await _collection.Find(m => m.ClienteGuid == clienteId).ToListAsync();
    }

    public async Task<Movimiento> AddMovimientoAsync(Movimiento movimiento)
    {
        _logger.LogInformation($"Adding new movimiento to the database: {movimiento}");
        await _collection.InsertOneAsync(movimiento);
        return movimiento;
    }

    public async Task<Movimiento> UpdateMovimientoAsync(String id, Movimiento movimiento)
    {
        _logger.LogInformation($"Updating movimiento with id: {id} in the database.");
        var updateResult = await _collection.FindOneAndReplaceAsync(
            m => m.Id == id,
            movimiento,
            new FindOneAndReplaceOptions<Movimiento>{ReturnDocument = ReturnDocument.After }
        );
        return updateResult;
    }

    public async Task<Movimiento> DeleteMovimientoAsync(String id)
    {
        _logger.LogInformation($"Deleting movimiento with id: {id} from the database.");
        var deletedMovimiento = await _collection.FindOneAndDeleteAsync(m => m.Id == id);
        return deletedMovimiento;
    }

    public async Task<List<Movimiento>> GetMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Getting movimientos de domiciliación for client with guid: {clienteGuid} from the database.");
        return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.Domiciliacion != null ).ToListAsync();
    }

    public async Task<List<Movimiento>> GetMovimientosTransferenciaByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Getting movimientos de transferencia for client with guid: {clienteGuid} from the database.");
        return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.Transferencia!= null ).ToListAsync();
    }

    public async Task<List<Movimiento>> GetMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid)
    {
        _logger.LogInformation($"Getting movimientos de pago con tarjeta for client with guid: {clienteGuid} from the database.");
        return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.PagoConTarjeta!= null ).ToListAsync();
    }
}