using System.Security.Claims;
using HotChocolate.Authorization;
using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Errors;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;

namespace VivesBankApi.Rest.Movimientos.Resolver;

public class MovimientosQuery(IMovimientoService movimientoService,IMovimientoMeQueriesService movimientoMeQueriesService, IDomiciliacionService domiciliacionService, IHttpContextAccessor httpContextAccessor)
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
            if (movimiento == null) throw new GraphQLException(new MovimientoNotFoundError(id));
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
        
        // [UsePaging]
        // [UseFiltering]
        // [UseSorting]
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
        
        [Authorize]
        public async Task<Movimiento> GetMovimientoByGuid(string guid)
        {
            var movimiento =  await movimientoService.FindMovimientoByGuidAsync(guid);
            if (movimiento == null) throw new GraphQLException(new MovimientoNotFoundError(guid));
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
        
        [Authorize]
        public async Task<IQueryable<Domiciliacion>> GetDomciliacionesActivasByCliente()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException("Debe estar autenticado para obtener las domiciliaciones activas.");
            }
            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var domiciliaciones = await domiciliacionService.FindDomiciliacionesActivasByClienteGiudAsync(guid);
            return domiciliaciones.Select(domiciliacion => new Domiciliacion
            {
                Guid = domiciliacion.Guid,
                ClienteGuid = domiciliacion.ClienteGuid,
                IbanOrigen = domiciliacion.IbanOrigen,
                IbanDestino = domiciliacion.IbanDestino,
                Cantidad = domiciliacion.Cantidad,
                NombreAcreedor = domiciliacion.NombreAcreedor,
                FechaInicio = domiciliacion.FechaInicio,
                Periodicidad = domiciliacion.Periodicidad,
                UltimaEjecucion = domiciliacion.UltimaEjecucion
            }).AsQueryable();
        }

        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosDomiciliacionByCliente()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException("Debe estar autenticado para obtener los movimientos de domiciliación.");
            }
            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoMeQueriesService.FindMovimientosDomiciliacionByClienteGuidAsync(guid);
            return movimientos.Select(movimiento => new Movimiento
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
}