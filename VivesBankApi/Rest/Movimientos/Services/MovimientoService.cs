using Microsoft.Extensions.Options;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories;
using VivesBankApi.Utils.ApiConfig;

namespace VivesBankApi.Rest.Movimientos.Services;

public class MovimientoService(IMovimientoRepository movimientoRepository, ILogger<MovimientoService> logger, IOptions<ApiConfig> apiConfig)
    : IMovimientoService
{
    public async Task<List<Movimiento>> FindAllMovimientosAsync()
    {
        logger.LogInformation("Finding all movimientos");
        return await movimientoRepository.GetAllMovimientosAsync();
    }

    public async Task<Movimiento> FindMovimientoByIdAsync(ObjectId id)
    {
        logger.LogInformation($"Finding movimiento by id: {id}");
        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(id);
        if (movimiento is null) throw new MovimientoNotFoundException(id);
        return movimiento;
    }

    public async Task<string> AddMovimientoAsync(Movimiento movimiento)
    {
        logger.LogInformation($"Adding movimiento: {movimiento}");
        var movimientoAdded = await movimientoRepository.AddMovimientoAsync(movimiento);
        return apiConfig.Value.BaseEndpoint + "/movimientos/" + movimientoAdded.Id;
    }

    public async Task<string> UpdateMovimientoAsync(ObjectId id, Movimiento movimiento)
    {
        logger.LogInformation($"Updating movimiento with id: {id}");
        var movimientoUpdated = await movimientoRepository.UpdateMovimientoAsync(id, movimiento);
        return apiConfig.Value.BaseEndpoint + "/movimientos/" + movimientoUpdated.Id;
    }

    public async Task<Movimiento> DeleteMovimientoAsync(ObjectId id)
    {
        logger.LogInformation($"Deleting movimiento with id: {id}");
        var deletedMovimiento = await movimientoRepository.DeleteMovimientoAsync(id);
        return deletedMovimiento;
    }
}