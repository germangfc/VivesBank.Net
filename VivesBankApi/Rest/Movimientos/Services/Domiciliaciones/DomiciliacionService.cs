using Microsoft.Extensions.Options;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Utils.ApiConfig;

namespace VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;

using Microsoft.Extensions.Options;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Utils.ApiConfig;

public class DomiciliacionService : IDomiciliacionService
{
    private readonly ILogger<DomiciliacionService> _logger;
    private readonly IDomiciliacionRepository _domiciliacionRepository;
    private readonly IOptions<ApiConfig> _apiConfig;
    
    public DomiciliacionService(IDomiciliacionRepository domiciliacionRepository, 
        ILogger<DomiciliacionService> logger, IOptions<ApiConfig> apiConfig)
    {
        _domiciliacionRepository = domiciliacionRepository;
        _logger = logger;
        _apiConfig = apiConfig;
    }

    /// <summary>
    /// Obtiene todas las domiciliaciones existentes.
    /// </summary>
    /// <remarks>
    /// Este método devuelve una lista con todas las domiciliaciones disponibles en el sistema.
    /// </remarks>
    /// <returns>Lista de domiciliaciones</returns>
    public async Task<List<Domiciliacion>> FindAllDomiciliacionesAsync()
    {
        _logger.LogInformation("Finding all Domiciliaciones");
        return await _domiciliacionRepository.GetAllDomiciliacionesAsync();
    }

    /// <summary>
    /// Obtiene una domiciliación por su ID.
    /// </summary>
    /// <param name="id">ID de la domiciliación</param>
    /// <remarks>
    /// Este método devuelve los detalles de una domiciliación específica identificada por su ID.
    /// Si no se encuentra la domiciliación, se lanzará una excepción.
    /// </remarks>
    /// <returns>Domiciliación correspondiente al ID</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    public async Task<Domiciliacion> FindDomiciliacionByIdAsync(String id)
    {
        _logger.LogInformation($"Finding domiciliacion by id: {id}");
        
        var domiciliacion = await _domiciliacionRepository.GetDomiciliacionByIdAsync(id);

        if (domiciliacion is null)
        {
            _logger.LogError($"Domiciliacion not found with id {id}");
            throw new DomiciliacionNotFoundException(id);
        }
        
        return domiciliacion;
    }

    /// <summary>
    /// Agrega una nueva domiciliación al sistema.
    /// </summary>
    /// <param name="domiciliacion">El objeto domiciliación que se va a agregar</param>
    /// <remarks>
    /// Este método permite agregar una nueva domiciliación al sistema. Devuelve la URL de la nueva domiciliación agregada.
    /// </remarks>
    /// <returns>URL de la domiciliación recién agregada</returns>
    public async Task<string> AddDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Adding domiciliacion {domiciliacion}");
        var domiciliacionAdded = await _domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);
        return _apiConfig.Value.BaseEndpoint + "/domiciliaciones/" + domiciliacionAdded.Id;
    }

    /// <summary>
    /// Actualiza una domiciliación existente en el sistema.
    /// </summary>
    /// <param name="id">ID de la domiciliación que se va a actualizar</param>
    /// <param name="domiciliacion">Objeto con la nueva información de la domiciliación</param>
    /// <remarks>
    /// Este método permite actualizar los detalles de una domiciliación existente. Si no se encuentra la domiciliación,
    /// se lanzará una excepción.
    /// </remarks>
    /// <returns>URL de la domiciliación actualizada</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    public async Task<string> UpdateDomiciliacionAsync(String id, Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Updating domiciliacion {domiciliacion} by id: {id}");
        
        var updatedDomiciliacion = await _domiciliacionRepository.UpdateDomiciliacionAsync(id, domiciliacion);

        if (updatedDomiciliacion is null)
        {
            _logger.LogError($"Domiciliacion not found with id {id}");
            throw new DomiciliacionNotFoundException(id);
        }
        
        return _apiConfig.Value.BaseEndpoint + "/domiciliaciones/" + updatedDomiciliacion.Id;
    }

    /// <summary>
    /// Elimina una domiciliación del sistema por su ID.
    /// </summary>
    /// <param name="id">ID de la domiciliación que se va a eliminar</param>
    /// <remarks>
    /// Este método elimina una domiciliación existente identificada por su ID. Si la domiciliación no existe,
    /// se lanzará una excepción.
    /// </remarks>
    /// <returns>Detalles de la domiciliación eliminada</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    public async Task<Domiciliacion> DeleteDomiciliacionAsync(String id)
    {
        _logger.LogInformation($"Deleting domiciliacion by id: {id} ");
        
        var deletedDomiciliacion = await _domiciliacionRepository.DeleteDomiciliacionAsync(id);

        if (deletedDomiciliacion is null)
        {
            _logger.LogError($"Domiciliacion not found with id {id}");
            throw new DomiciliacionNotFoundException(id);
        }
        
        return deletedDomiciliacion;
    }

    /// <summary>
    /// Obtiene las domiciliaciones activas de un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve todas las domiciliaciones activas asociadas al cliente identificado por su GUID.
    /// </remarks>
    /// <returns>Lista de domiciliaciones activas para el cliente</returns>
    public async Task<List<Domiciliacion>> FindDomiciliacionesActivasByClienteGiudAsync(string clienteGuid)
    {
        _logger.LogInformation($"Finding domiciliaciones activas by cliente guid: {clienteGuid}");
        return await _domiciliacionRepository.GetDomiciliacionesActivasByClienteGiudAsync(clienteGuid);
    }
}
