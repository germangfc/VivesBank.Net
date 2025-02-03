using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Movimientos
{
    /// <summary>
    /// Implementación de la interfaz <see cref="IMovimientoRepository"/> para gestionar los movimientos bancarios en la base de datos MongoDB.
    /// Esta clase contiene métodos para realizar operaciones CRUD sobre los movimientos, así como consultas específicas relacionadas con el cliente y el tipo de movimiento.
    /// </summary>
    /// <remarks>
    /// Utiliza la colección de MongoDB especificada para acceder, agregar, actualizar y eliminar movimientos en la base de datos.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class MovimientoRepository : IMovimientoRepository
    {
        private readonly IMongoCollection<Movimiento> _collection;
        private readonly ILogger<MovimientoRepository> _logger;

        /// <summary>
        /// Constructor que inicializa la conexión a la base de datos y la colección de movimientos.
        /// </summary>
        /// <param name="mongoDatabaseSettings">Configuración de la base de datos MongoDB.</param>
        /// <param name="logger">Instancia de logger para registrar información y errores.</param>
        public MovimientoRepository(IOptions<MongoDatabaseConfig> mongoDatabaseSettings, ILogger<MovimientoRepository> logger)
        {
            var client = new MongoClient(mongoDatabaseSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDatabaseSettings.Value.DatabaseName);
            _collection = database.GetCollection<Movimiento>(mongoDatabaseSettings.Value.MovimientosCollectionName);
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los movimientos registrados en la base de datos.
        /// </summary>
        /// <returns>Una lista de todos los movimientos.</returns>
        public async Task<List<Movimiento>> GetAllMovimientosAsync()
        {
            _logger.LogInformation("Getting all movimientos from the database.");
            return await _collection.Find(_ => true).ToListAsync();
        }

        /// <summary>
        /// Obtiene un movimiento específico por su id.
        /// </summary>
        /// <param name="id">El id del movimiento a obtener.</param>
        /// <returns>El movimiento con el id especificado o null si no se encuentra.</returns>
        public async Task<Movimiento?> GetMovimientoByIdAsync(String id)
        {
            _logger.LogInformation($"Getting movimiento with id: {id} from the database.");
            return await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene un movimiento específico por su GUID.
        /// </summary>
        /// <param name="guid">El GUID del movimiento a obtener.</param>
        /// <returns>El movimiento con el GUID especificado o null si no se encuentra.</returns>
        public async Task<Movimiento?> GetMovimientoByGuidAsync(string guid)
        {
            _logger.LogInformation($"Getting movimiento with guid: {guid} from the database.");
            return await _collection.Find(m => m.Guid == guid).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene todos los movimientos asociados a un cliente específico por su GUID.
        /// </summary>
        /// <param name="clienteId">El GUID del cliente para filtrar los movimientos.</param>
        /// <returns>Una lista de movimientos asociados al cliente.</returns>
        public async Task<List<Movimiento>> GetMovimientosByClientAsync(string clienteId)
        {
            _logger.LogInformation($"Getting movimientos for client with id: {clienteId} from the database.");
            return await _collection.Find(m => m.ClienteGuid == clienteId).ToListAsync();
        }

        /// <summary>
        /// Agrega un nuevo movimiento a la base de datos.
        /// </summary>
        /// <param name="movimiento">El objeto <see cref="Movimiento"/> a agregar.</param>
        /// <returns>El movimiento que fue agregado.</returns>
        public async Task<Movimiento> AddMovimientoAsync(Movimiento movimiento)
        {
            _logger.LogInformation($"Adding new movimiento to the database: {movimiento}");
            await _collection.InsertOneAsync(movimiento);
            return movimiento;
        }

        /// <summary>
        /// Actualiza un movimiento existente en la base de datos.
        /// </summary>
        /// <param name="id">El id del movimiento a actualizar.</param>
        /// <param name="movimiento">El objeto <see cref="Movimiento"/> con los nuevos datos.</param>
        /// <returns>El movimiento actualizado.</returns>
        public async Task<Movimiento> UpdateMovimientoAsync(String id, Movimiento movimiento)
        {
            _logger.LogInformation($"Updating movimiento with id: {id} in the database.");
            var updateResult = await _collection.FindOneAndReplaceAsync(
                m => m.Id == id,
                movimiento,
                new FindOneAndReplaceOptions<Movimiento> { ReturnDocument = ReturnDocument.After }
            );
            return updateResult;
        }

        /// <summary>
        /// Elimina un movimiento de la base de datos.
        /// </summary>
        /// <param name="id">El id del movimiento a eliminar.</param>
        /// <returns>El movimiento eliminado.</returns>
        public async Task<Movimiento> DeleteMovimientoAsync(String id)
        {
            _logger.LogInformation($"Deleting movimiento with id: {id} from the database.");
            var deletedMovimiento = await _collection.FindOneAndDeleteAsync(m => m.Id == id);
            return deletedMovimiento;
        }

        /// <summary>
        /// Obtiene todos los movimientos de domiciliación asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de domiciliación.</param>
        /// <returns>Una lista de movimientos de domiciliación asociados al cliente.</returns>
        public async Task<List<Movimiento>> GetMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting movimientos de domiciliación for client with guid: {clienteGuid} from the database.");
            return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.Domiciliacion != null).ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los movimientos de transferencia asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de transferencia.</param>
        /// <returns>Una lista de movimientos de transferencia asociados al cliente.</returns>
        public async Task<List<Movimiento>> GetMovimientosTransferenciaByClienteGuidAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting movimientos de transferencia for client with guid: {clienteGuid} from the database.");
            return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.Transferencia != null).ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los movimientos de pago con tarjeta asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de pago con tarjeta.</param>
        /// <returns>Una lista de movimientos de pago con tarjeta asociados al cliente.</returns>
        public async Task<List<Movimiento>> GetMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting movimientos de pago con tarjeta for client with guid: {clienteGuid} from the database.");
            return await _collection.Find(m => m.ClienteGuid == clienteGuid && m.PagoConTarjeta != null).ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los movimientos de recibo de nómina asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de recibo de nómina.</param>
        /// <returns>Una lista de movimientos de recibo de nómina asociados al cliente.</returns>
        public Task<List<Movimiento>> GetMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting movimientos de recibo de nómina for client with guid: {clienteGuid} from the database.");
            return _collection.Find(m => m.ClienteGuid == clienteGuid && m.IngresoDeNomina != null).ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los movimientos de transferencia revocada asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de transferencia revocada.</param>
        /// <returns>Una lista de movimientos de transferencia revocada asociados al cliente.</returns>
        public Task<List<Movimiento>> GetMovimientosTransferenciaRevocadaByClienteGuidAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting movimientos de transferencia revocada for client with guid: {clienteGuid} from the database.");
            return _collection.Find(m => m.ClienteGuid == clienteGuid && m.IsDeleted).ToListAsync();
        }
    }
}
