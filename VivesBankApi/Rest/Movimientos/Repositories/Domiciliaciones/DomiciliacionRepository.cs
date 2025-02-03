using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using VivesBankApi.Rest.Movimientos.Config;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones
{
    /// <summary>
    /// Implementación del repositorio para gestionar las domiciliaciones en la base de datos MongoDB.
    /// </summary>
    /// <remarks>
    /// Esta clase se encarga de las operaciones CRUD (crear, leer, actualizar y eliminar) relacionadas con las domiciliaciones.
    /// Utiliza la colección de MongoDB para realizar estas operaciones sobre los documentos de domiciliación.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public class DomiciliacionRepository : IDomiciliacionRepository
    {
        private readonly IMongoCollection<Domiciliacion> _collection;
        private readonly ILogger<DomiciliacionRepository> _logger;

        /// <summary>
        /// Constructor que inicializa la clase <see cref="DomiciliacionRepository"/> con la configuración de la base de datos MongoDB y el logger.
        /// </summary>
        /// <param name="mongoDatabaseSettings">Configuraciones de la base de datos MongoDB desde el archivo de configuración.</param>
        /// <param name="logger">Instancia de logger para registrar información de la ejecución.</param>
        public DomiciliacionRepository(IOptions<MongoDatabaseConfig> mongoDatabaseSettings, ILogger<DomiciliacionRepository> logger)
        {
            var client = new MongoClient(mongoDatabaseSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoDatabaseSettings.Value.DatabaseName);
            _collection = database.GetCollection<Domiciliacion>(mongoDatabaseSettings.Value.DomiciliacionCollectionName);
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las domiciliaciones de la base de datos.
        /// </summary>
        /// <returns>Una lista de todas las domiciliaciones.</returns>
        public async Task<List<Domiciliacion>> GetAllDomiciliacionesAsync()
        {
            _logger.LogInformation("Getting all direct debits from the database.");
            return await _collection.FindAsync(_ => true).Result.ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las domiciliaciones activas de la base de datos.
        /// </summary>
        /// <returns>Una lista de domiciliaciones activas.</returns>
        public async Task<List<Domiciliacion>> GetAllDomiciliacionesActivasAsync()
        {
            _logger.LogInformation("Getting all active direct debits from the database.");
            return await _collection.FindAsync(d => d.Activa).Result.ToListAsync();
        }

        /// <summary>
        /// Obtiene una domiciliación específica por su id.
        /// </summary>
        /// <param name="id">El id de la domiciliación a obtener.</param>
        /// <returns>La domiciliación con el id especificado o null si no se encuentra.</returns>
        public async Task<Domiciliacion> GetDomiciliacionByIdAsync(string id)
        {
            _logger.LogInformation($"Getting direct debit with id {id} from the database.");
            return await _collection.FindAsync(d => d.Id == id).Result.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Agrega una nueva domiciliación a la base de datos.
        /// </summary>
        /// <param name="domiciliacion">El objeto <see cref="Domiciliacion"/> a agregar.</param>
        /// <returns>La domiciliación que fue agregada.</returns>
        public async Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion)
        {
            _logger.LogInformation($"Adding a new direct debit to the database: {domiciliacion}");
            await _collection.InsertOneAsync(domiciliacion);
            return domiciliacion;
        }

        /// <summary>
        /// Actualiza una domiciliación existente en la base de datos.
        /// </summary>
        /// <param name="id">El id de la domiciliación a actualizar.</param>
        /// <param name="domiciliacion">El objeto <see cref="Domiciliacion"/> con los nuevos datos.</param>
        /// <returns>La domiciliación actualizada.</returns>
        public async Task<Domiciliacion> UpdateDomiciliacionAsync(string id, Domiciliacion domiciliacion)
        {
            _logger.LogInformation($"Updating direct debit with id {id} in the database.");
            var updateResult = await _collection.FindOneAndReplaceAsync(
                d => d.Id == id,
                domiciliacion,
                new FindOneAndReplaceOptions<Domiciliacion>{ ReturnDocument = ReturnDocument.After }
            );
            return updateResult;
        }

        /// <summary>
        /// Elimina una domiciliación de la base de datos.
        /// </summary>
        /// <param name="id">El id de la domiciliación a eliminar.</param>
        /// <returns>La domiciliación eliminada.</returns>
        public async Task<Domiciliacion> DeleteDomiciliacionAsync(string id)
        {
            _logger.LogInformation($"Deleting direct debit with id {id} from the database.");
            var deletedDomiciliacion = await _collection.FindOneAndDeleteAsync(d => d.Id == id);
            return deletedDomiciliacion;
        }

        /// <summary>
        /// Obtiene todas las domiciliaciones activas de un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar las domiciliaciones activas.</param>
        /// <returns>Una lista de domiciliaciones activas asociadas al cliente.</returns>
        public async Task<List<Domiciliacion>> GetDomiciliacionesActivasByClienteGiudAsync(string clienteGuid)
        {
            _logger.LogInformation($"Getting active direct debits for client with guid {clienteGuid} from the database.");
            return await _collection.FindAsync(d => d.ClienteGuid == clienteGuid && d.Activa).Result.ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las domiciliaciones de un cliente específico.
        /// </summary>
        /// <param name="clientGuid">El GUID del cliente para filtrar las domiciliaciones.</param>
        /// <returns>Una lista de domiciliaciones asociadas al cliente.</returns>
        public async Task<List<Domiciliacion>> GetDomiciliacionByClientGuidAsync(string clientGuid)
        {
            _logger.LogInformation($"Getting direct debits for client with guid {clientGuid}.");
            return await _collection.FindAsync(d => d.ClienteGuid == clientGuid).Result.ToListAsync();
        }
    }
}
