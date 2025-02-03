using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public class MovimientoMeQueriesService : IMovimientoMeQueriesService
{
    private readonly ILogger<MovimientoMeQueriesService> logger;
    private readonly IMovimientoRepository movimientoRepository;

    public MovimientoMeQueriesService(ILogger<MovimientoMeQueriesService> logger, IMovimientoRepository movimientoRepository)
    {
        this.logger = logger;
        this.movimientoRepository = movimientoRepository;
    }

    /// <summary>
    /// Obtiene los movimientos relacionados con domiciliaciones para un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve los movimientos de domiciliación asociados a un cliente específico mediante su GUID.
    /// </remarks>
    /// <returns>Lista de movimientos de domiciliación del cliente</returns>
    public async Task<List<Movimiento>> FindMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos domiciliación for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosDomiciliacionByClienteGuidAsync(clienteGuid);
    }

    /// <summary>
    /// Obtiene los movimientos relacionados con transferencias para un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve los movimientos de transferencia asociados a un cliente específico mediante su GUID.
    /// </remarks>
    /// <returns>Lista de movimientos de transferencia del cliente</returns>
    public async Task<List<Movimiento>> FindMovimientosTransferenciaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos transferencia for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosTransferenciaByClienteGuidAsync(clienteGuid);
    }

    /// <summary>
    /// Obtiene los movimientos relacionados con pagos con tarjeta para un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve los movimientos de pago con tarjeta asociados a un cliente específico mediante su GUID.
    /// </remarks>
    /// <returns>Lista de movimientos de pagos con tarjeta del cliente</returns>
    public async Task<List<Movimiento>> FindMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos pago con tarjeta for client with GUID: {clienteGuid}");
        return await movimientoRepository.GetMovimientosPagoConTarjetaByClienteGuidAsync(clienteGuid);
    }

    /// <summary>
    /// Obtiene los movimientos relacionados con recibos de nómina para un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve los movimientos de recibo de nómina asociados a un cliente específico mediante su GUID.
    /// </remarks>
    /// <returns>Lista de movimientos de recibo de nómina del cliente</returns>
    public Task<List<Movimiento>> FindMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos recibo de nomina for client with GUID: {clienteGuid}");
        return movimientoRepository.GetMovimientosReciboDeNominaByClienteGuidAsync(clienteGuid);
    }

    /// <summary>
    /// Obtiene los movimientos relacionados con transferencias revocadas para un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve los movimientos de transferencias revocadas asociadas a un cliente específico mediante su GUID.
    /// </remarks>
    /// <returns>Lista de movimientos de transferencias revocadas del cliente</returns>
    public Task<List<Movimiento>> FindMovimientosTransferenciaRevocadaClienteGuidAsync(string clienteGuid)
    {
        logger.LogInformation($"Finding movimientos transferencia revocada for client with GUID: {clienteGuid}");
        return movimientoRepository.GetMovimientosTransferenciaRevocadaByClienteGuidAsync(clienteGuid);
    }
}