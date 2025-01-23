using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Movimientos;

public interface IMovimientoRepository
{
    Task<List<Movimiento>> GetAllMovimientosAsync();
    Task<Movimiento> GetMovimientoByIdAsync(String id);
    Task<Movimiento> GetMovimientoByGuidAsync(string guid);
    Task<List<Movimiento>> GetMovimientosByClientAsync(string clienteId);
    Task<Movimiento> AddMovimientoAsync(Movimiento movimiento);
    Task<Movimiento> UpdateMovimientoAsync(String id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(String id);
    Task<List<Movimiento>> GetMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid);
    Task<List<Movimiento>> GetMovimientosTransferenciaByClienteGuidAsync(string clienteGuid);
    Task<List<Movimiento>> GetMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid);
    Task<List<Movimiento>> GetMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid);
}