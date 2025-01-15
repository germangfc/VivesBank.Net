using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Movimientos.Services;

public interface IMovimientoService
{
    Task<List<Movimiento>> FindAllMovimientosAsync();
    Task<Movimiento> FindMovimientoByIdAsync(ObjectId id);
    Task<Movimiento> FindMovimientoByGuidAsync(string guid);
    Task<List<Movimiento>> FindAllMovimientosByClientAsync(string clienteId);
    Task<String> AddMovimientoAsync(Movimiento movimiento);
    Task<String> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(ObjectId id);
    
    Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion);
    
    Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina);
    
    Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta);
    
    Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia);
    
    Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid);
}