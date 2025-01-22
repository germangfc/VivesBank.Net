using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public interface IMovimientoMeQueriesService
{
    Task<List<Movimiento>> FindMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid);
    Task<List<Movimiento>> FindMovimientosTransferenciaByClienteGuidAsync(string clienteGuid);
    Task<List<Movimiento>> FindMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid);


}