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

        /// <summary>
        /// Obtiene todos los movimientos registrados en el sistema.
        /// </summary>
        /// <remarks>
        /// Este endpoint devuelve una lista de todos los movimientos disponibles. 
        /// Requiere permisos de administrador.
        /// </remarks>
        /// <returns>Lista de movimientos</returns>
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
        
        /// <summary>
        /// Obtiene un movimiento específico por su ID.
        /// </summary>
        /// <param name="id">El ID del movimiento a recuperar</param>
        /// <remarks>
        /// Si el movimiento no se encuentra, se lanzará un error.
        /// </remarks>
        /// <returns>Movimiento encontrado</returns>
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
        
        /// <summary>
        /// Obtiene los movimientos de un cliente específico basado en su GUID.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos</param>
        /// <returns>Lista de movimientos para ese cliente</returns>
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
        
        
        /// <summary>
        /// Obtiene un movimiento específico por su GUID.
        /// </summary>
        /// <param name="guid">El GUID del movimiento a recuperar</param>
        /// <remarks>
        /// Si el movimiento no se encuentra, se lanzará un error.
        /// </remarks>
        /// <returns>Movimiento encontrado</returns>
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
        
        /// <summary>
        /// Obtiene las domiciliaciones activas de un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para usuarios autenticados. Si el usuario no está autenticado, se lanza un error.
        /// </remarks>
        /// <returns>Lista de domiciliaciones activas del cliente</returns>
        [Authorize]
        public async Task<IQueryable<Domiciliacion>> GetDomciliacionesActivasByCliente()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
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

        /// <summary>
        /// Obtiene los movimientos relacionados con domiciliaciones para el cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados.
        /// </remarks>
        /// <returns>Lista de movimientos de domiciliación para el cliente</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosDomiciliacionByCliente()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
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

        
        /// <summary>
        /// Obtiene los movimientos de transferencia relacionados con el cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados. 
        /// Si el cliente no está autenticado, se lanzará un error de autenticación.
        /// </remarks>
        /// <returns>Lista de movimientos de transferencia del cliente autenticado</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosTransferenciaByClienteGuidAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
            }
            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoMeQueriesService.FindMovimientosTransferenciaByClienteGuidAsync(guid);
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

        /// <summary>
        /// Obtiene los movimientos de pago con tarjeta relacionados con el cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados. 
        /// Si el cliente no está autenticado, se lanzará un error de autenticación.
        /// </remarks>
        /// <returns>Lista de movimientos de pago con tarjeta del cliente autenticado</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosPagoConTarjetaByClienteGuidAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
            }
            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoMeQueriesService.FindMovimientosPagoConTarjetaByClienteGuidAsync(guid);
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
        
        /// <summary>
        /// Obtiene los movimientos de ingreso de nómina relacionados con el cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados. 
        /// Si el cliente no está autenticado, se lanzará un error de autenticación.
        /// </remarks>
        /// <returns>Lista de movimientos de ingreso de nómina del cliente autenticado</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosIngresoDeNominaByClienteGuidAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null ||!user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
            }
            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoMeQueriesService.FindMovimientosReciboDeNominaByClienteGuidAsync(guid);
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

        /// <summary>
        /// Obtiene todos los movimientos de un cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados. 
        /// Si el cliente no está autenticado, se lanzará un error de autenticación.
        /// </remarks>
        /// <returns>Lista de todos los movimientos del cliente autenticado</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosMeByClienteGuidAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
            }

            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoService.FindAllMovimientosByClientAsync(guid);
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
        
        /// <summary>
        /// Obtiene los movimientos de transferencia revocada relacionados con el cliente autenticado.
        /// </summary>
        /// <remarks>
        /// Este endpoint solo está disponible para clientes autenticados. 
        /// Si el cliente no está autenticado, se lanzará un error de autenticación.
        /// </remarks>
        /// <returns>Lista de movimientos de transferencia revocada del cliente autenticado</returns>
        [Authorize]
        public async Task<IQueryable<Movimiento>> GetMovimientosTransferenciaRevocadaByClienteGuidAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                throw new GraphQLException(new UserNotAuthenticatedError());
            }

            var guid = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var movimientos = await movimientoMeQueriesService.FindMovimientosTransferenciaRevocadaClienteGuidAsync(guid);
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