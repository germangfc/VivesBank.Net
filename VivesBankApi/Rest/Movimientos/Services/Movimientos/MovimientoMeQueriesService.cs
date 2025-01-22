using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public class MovimientoMeQueriesService(ILogger<MovimientoMeQueriesService> logger, IMovimientoRepository movimientoRepository) : IMovimientoMeQueriesService
{
    public async Task<List<Movimiento>> FindMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos domiciliación for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosDomiciliacionByClienteGuidAsync(clienteGuid);
    }
}