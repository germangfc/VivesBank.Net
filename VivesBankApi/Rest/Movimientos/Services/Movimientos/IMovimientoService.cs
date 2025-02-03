using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public interface IMovimientoService : IGenericStorageJson<Movimiento>
{
    Task<List<Movimiento>> FindAllMovimientosAsync();
    Task<Movimiento> FindMovimientoByIdAsync(String id);
    Task<Movimiento> FindMovimientoByGuidAsync(string guid);
    Task<List<Movimiento>> FindAllMovimientosByClientAsync(string clienteId);
    Task<Movimiento> AddMovimientoAsync(Movimiento movimiento);
    Task<Movimiento> UpdateMovimientoAsync(String id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(String id);
    
    Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion);
    
    Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina);
    
    Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta);
    
    Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia);
    
    Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid);
}