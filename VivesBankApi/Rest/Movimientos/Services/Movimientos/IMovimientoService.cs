using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public interface IMovimientoService
{
    Task<List<Movimiento>> FindAllMovimientosAsync();
    Task<Movimiento> FindMovimientoByIdAsync(ObjectId id);
    Task<String> AddMovimientoAsync(Movimiento movimiento);
    Task<String> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(ObjectId id);
}