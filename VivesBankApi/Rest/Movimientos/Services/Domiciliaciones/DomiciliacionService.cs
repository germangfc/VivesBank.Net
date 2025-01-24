using Microsoft.Extensions.Options;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Utils.ApiConfig;

namespace VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;

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

    public async Task<List<Domiciliacion>> FindAllDomiciliacionesAsync()
    {
        _logger.LogInformation("Finding all Domiciliaciones");
        return await _domiciliacionRepository.GetAllDomiciliacionesAsync();
    }

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

    public async Task<string> AddDomiciliacionAsync(Domiciliacion domiciliacion)
    {
        _logger.LogInformation($"Adding domiciliacion {domiciliacion}");
        var domiciliacionAdded = await _domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);
        return _apiConfig.Value.BaseEndpoint + "/domiciliaciones/" + domiciliacionAdded.Id;
    }

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

    public async Task<List<Domiciliacion>> FindDomiciliacionesActivasByClienteGiudAsync(string clienteGuid)
    {
        _logger.LogInformation($"Finding domiciliaciones activas by cliente guid: {clienteGuid}");
        return await _domiciliacionRepository.GetDomiciliacionesActivasByClienteGiudAsync(clienteGuid);
    }
}