using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services;

public class MovimientoService : IMovimientoService
{
    public async Task<List<Movimiento>> GetAllMovimientosAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> GetMovimientoByIdAsync(ObjectId id)
    {
        throw new NotImplementedException();
    }

    public async Task<string> AddMovimientoAsync(Movimiento movimiento)
    {
        throw new NotImplementedException();
    }

    public async Task<string> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento)
    {
        throw new NotImplementedException();
    }

    public async Task<Movimiento> DeleteMovimientoAsync(ObjectId id)
    {
        throw new NotImplementedException();
    }
}