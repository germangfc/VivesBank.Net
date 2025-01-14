using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services;

public interface IMovimientoService
{
    Task<List<Movimiento>> GetAllMovimientosAsync();
    Task<Movimiento> GetMovimientoByIdAsync(ObjectId id);
    Task<String> AddMovimientoAsync(Movimiento movimiento);
    Task<String> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(ObjectId id);
}