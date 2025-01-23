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

    public async Task<List<Movimiento>> FindMovimientosTransferenciaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos transferencia for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosTransferenciaByClienteGuidAsync(clienteGuid);
    }

    public async Task<List<Movimiento>> FindMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos pago con tarjeta for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosPagoConTarjetaByClienteGuidAsync(clienteGuid);
    }

    public Task<List<Movimiento>> FindMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos recibo de nomina for client with GUID: {clienteGuid}");
        return movimientoRepository.GetMovimientosReciboDeNominaByClienteGuidAsync(clienteGuid);
    }
}