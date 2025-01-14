using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories;

public interface IMovimientoRepository
{
    Task<List<Movimiento>> GetAllMovimientosAsync();
    Task<Movimiento> GetMovimientoByIdAsync(ObjectId id);
    Task<Movimiento> AddMovimientoAsync(Movimiento movimiento);
    Task<Movimiento> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento);
    Task<Movimiento> DeleteMovimientoAsync(ObjectId id);
}