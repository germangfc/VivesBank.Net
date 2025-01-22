using Microsoft.Extensions.Options;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Validators;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Utils.ApiConfig;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public class MovimientoService(IMovimientoRepository movimientoRepository, IDomiciliacionRepository domiciliacionRepository, ILogger<MovimientoService> logger, IOptions<ApiConfig> apiConfig)
    : IMovimientoService
{
    public async Task<List<Movimiento>> FindAllMovimientosAsync()
    {
        logger.LogInformation("Finding all movimientos");
        return await movimientoRepository.GetAllMovimientosAsync();
    }

    public async Task<Movimiento> FindMovimientoByIdAsync(String id)
    {
        logger.LogInformation($"Finding movimiento by id: {id}");
        var movimiento = await movimientoRepository.GetMovimientoByIdAsync(id);
        if (movimiento is null) throw new MovimientoNotFoundException(id);
        return movimiento;
    }

    public async Task<Movimiento> FindMovimientoByGuidAsync(string guid)
    {
        logger.LogInformation($"Finding movimiento by guid: {guid}");
        var movimiento = await movimientoRepository.GetMovimientoByGuidAsync(guid);
        if (movimiento is null) throw new MovimientoNotFoundException(guid);
        return movimiento;
    }

    public async Task<List<Movimiento>> FindAllMovimientosByClientAsync(string clienteId)
    {
        logger.LogInformation($"Finding movimientos by client id: {clienteId}");
        return await movimientoRepository.GetMovimientosByClientAsync(clienteId);
    }

    public async Task<Movimiento> AddMovimientoAsync(Movimiento movimiento)
    {
        logger.LogInformation($"Adding movimiento: {movimiento}");
        return await movimientoRepository.AddMovimientoAsync(movimiento);
    }

    public async Task<Movimiento> UpdateMovimientoAsync(String id, Movimiento movimiento)
    {
        logger.LogInformation($"Updating movimiento with id: {id}");
        return await movimientoRepository.UpdateMovimientoAsync(id, movimiento);
    }

    public async Task<Movimiento> DeleteMovimientoAsync(String id)
    {
        logger.LogInformation($"Deleting movimiento with id: {id}");
        var deletedMovimiento = await movimientoRepository.DeleteMovimientoAsync(id);
        return deletedMovimiento;
    }

    public async Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion)
    {
        logger.LogInformation($"Adding domiciliacion cantidad: {domiciliacion.Cantidad}");
        // Validar que la cantidad es mayor que cero
        var cantidad = domiciliacion.Cantidad;
        if (cantidad <= 0) throw new DomiciliacionCantidadInvalidaException(domiciliacion.Id, domiciliacion.Cantidad);
        
        // validar Iban correcto
        if (!IbanValidator.ValidateIban(domiciliacion.IbanDestino)) throw new IbanDestinoInvalidoException(domiciliacion.IbanDestino);
        if (!IbanValidator.ValidateIban(domiciliacion.IbanOrigen)) throw new IbanOrigenInvalidoException(domiciliacion.IbanOrigen);

        // Validar que el cliente existe
        
        // Validar que la cuenta existe
        // Validar si la domiciliación ya existe
        // Guardar la domiciliación
        // Notificación al cliente
        // Retornar respuesta

        return await domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);

    }

    public Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina)
    {
        throw new NotImplementedException();
    }

    public Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta)
    {
        throw new NotImplementedException();
    }

    public Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia)
    {
        throw new NotImplementedException();
    }

    public Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid)
    {
        throw new NotImplementedException();
    }
}