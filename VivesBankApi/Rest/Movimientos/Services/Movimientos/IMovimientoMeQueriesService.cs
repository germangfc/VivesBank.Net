using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public interface IMovimientoMeQueriesService
{
    /// <summary>
    /// Obtiene los movimientos de domiciliación para un cliente identificado por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos de domiciliación asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos de domiciliación</returns>
    Task<List<Movimiento>> FindMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid);

    /// <summary>
    /// Obtiene los movimientos de transferencia para un cliente identificado por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos de transferencia asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos de transferencia</returns>
    Task<List<Movimiento>> FindMovimientosTransferenciaByClienteGuidAsync(string clienteGuid);

    /// <summary>
    /// Obtiene los movimientos de pago con tarjeta para un cliente identificado por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos de pago con tarjeta asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos de pago con tarjeta</returns>
    Task<List<Movimiento>> FindMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid);

    /// <summary>
    /// Obtiene los movimientos de recibo de nómina para un cliente identificado por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos de recibo de nómina asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos de recibo de nómina</returns>
    Task<List<Movimiento>> FindMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid);

    /// <summary>
    /// Obtiene los movimientos de transferencia revocada para un cliente identificado por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos de transferencia revocada asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos de transferencia revocada</returns>
    Task<List<Movimiento>> FindMovimientosTransferenciaRevocadaClienteGuidAsync(string clienteGuid);
}
