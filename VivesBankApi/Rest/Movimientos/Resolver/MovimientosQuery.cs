using HotChocolate.Authorization;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;

namespace VivesBankApi.Rest.Movimientos.Resolver;

public class MovimientosQuery(IMovimientoService movimientoService)
{

        // [UsePaging]
        // [UseFiltering]
        // [UseSorting]
        // [Authorize(Policy = "Admin")]
        public async Task<IQueryable<Movimiento>> GetMovimientos()
        {
            var movimientosList = await movimientoService.FindAllMovimientosAsync();
            return movimientosList.Select(movimiento => new Movimiento
            {
                Guid = movimiento.Guid,
                ClienteGuid = movimiento.ClienteGuid,
                Domiciliacion = movimiento.Domiciliacion,
                IngresoDeNomina = movimiento.IngresoDeNomina,
                PagoConTarjeta = movimiento.PagoConTarjeta,
                Transferencia = movimiento.Transferencia,
                CreatedAt = movimiento.CreatedAt,
                UpdatedAt = movimiento.UpdatedAt
            }).AsQueryable();
        }
        
        //[Authorize(Policy = "Admin")]
        public async Task<Movimiento> GetMovimientoById(String id)
        {
            var movimiento =  await movimientoService.FindMovimientoByIdAsync(id);
            return new Movimiento
            {
                Guid = movimiento.Guid,
                ClienteGuid = movimiento.ClienteGuid,
                Domiciliacion = movimiento.Domiciliacion,
                IngresoDeNomina = movimiento.IngresoDeNomina,
                PagoConTarjeta = movimiento.PagoConTarjeta,
                Transferencia = movimiento.Transferencia,
                CreatedAt = movimiento.CreatedAt,
                UpdatedAt = movimiento.UpdatedAt
            };
        }
        
        [UsePaging]
        [UseFiltering]
        [UseSorting]
       // [Authorize(Policy = "User")]
        public async Task<IQueryable<Movimiento>> GetMovimientosByCliente(string clienteGuid)
        {
            var movimientosList = await movimientoService.FindAllMovimientosByClientAsync(clienteGuid);
            return movimientosList.Select(
                movimiento => new Movimiento
                {
                    Guid = movimiento.Guid,
                    Domiciliacion = movimiento.Domiciliacion,
                    IngresoDeNomina = movimiento.IngresoDeNomina,
                    PagoConTarjeta = movimiento.PagoConTarjeta,
                    Transferencia = movimiento.Transferencia,
                    CreatedAt = movimiento.CreatedAt,
                    UpdatedAt = movimiento.UpdatedAt
                }
            ).AsQueryable();
        }
        
        //[Authorize(Policy = "Admin")]
        public async Task<Movimiento> GetMovimientoByGuid(string guid)
        {
            var movimiento =  await movimientoService.FindMovimientoByGuidAsync(guid);
            return new Movimiento
            {
                ClienteGuid = movimiento.ClienteGuid,
                Domiciliacion = movimiento.Domiciliacion,
                IngresoDeNomina = movimiento.IngresoDeNomina,
                PagoConTarjeta = movimiento.PagoConTarjeta,
                Transferencia = movimiento.Transferencia,
                CreatedAt = movimiento.CreatedAt,
                UpdatedAt = movimiento.UpdatedAt
            };
        }
}