using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace VivesBankApi.Rest.Movimientos.Controller
{
    /// <summary>
    /// Controlador para manejar los movimientos bancarios de los usuarios.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientoController : ControllerBase
    {
        private readonly IMovimientoService _movimientoService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<MovimientoController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Constructor del controlador que inyecta las dependencias necesarias.
        /// </summary>
        /// <param name="movimientoService">Servicio para gestionar los movimientos bancarios.</param>
        /// <param name="userRepository">Repositorio para gestionar los usuarios.</param>
        /// <param name="logger">Logger para registrar información y errores.</param>
        /// <param name="httpContextAccessor">Acceso al contexto HTTP para obtener información del usuario autenticado.</param>
        public MovimientoController(
            IMovimientoService movimientoService,
            IUserRepository userRepository,
            ILogger<MovimientoController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _movimientoService = movimientoService;
            _userRepository = userRepository;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Crea una nueva domiciliación para el usuario autenticado.
        /// </summary>
        /// <param name="domiciliacion">Objeto con los datos de la domiciliación a crear.</param>
        /// <returns>El objeto de la domiciliación creada.</returns>
        [Authorize]
        [HttpPost("Domiciliacion/")]
        public async Task<ActionResult<Domiciliacion>> CreateDomiciliacion([FromBody] Domiciliacion domiciliacion)
        {
            _logger.LogInformation("Creating new domiciliacion");
            var user = _httpContextAccessor.HttpContext!.User;
            var appUser = await ConvertClaimsPrincipalToUser(user);
            return await _movimientoService.AddDomiciliacionAsync(appUser, domiciliacion);
        }

        /// <summary>
        /// Crea una nueva transferencia para el usuario autenticado.
        /// </summary>
        /// <param name="transferencia">Objeto con los datos de la transferencia a crear.</param>
        /// <returns>El objeto de la transferencia creada.</returns>
        [Authorize]
        [HttpPost("Transferencia/")]
        public async Task<ActionResult<Movimiento>> AddTransferencia([FromBody] Transferencia transferencia)
        {
            _logger.LogInformation("Creating new transferencia");
            var user = _httpContextAccessor.HttpContext!.User;
            var appUser = await ConvertClaimsPrincipalToUser(user);
            return await _movimientoService.AddTransferenciaAsync(appUser, transferencia);
        }

        /// <summary>
        /// Crea un nuevo ingreso de nómina para el usuario autenticado.
        /// </summary>
        /// <param name="ingresoDeNomina">Objeto con los datos del ingreso de nómina a crear.</param>
        /// <returns>El objeto del ingreso de nómina creado.</returns>
        [Authorize]
        [HttpPost("Ingresonomina/")]
        public async Task<ActionResult<Movimiento>> AddIngresoDeNomina([FromBody] IngresoDeNomina ingresoDeNomina)
        {
            _logger.LogInformation("Creating new ingreso de nomina");
            var user = _httpContextAccessor.HttpContext!.User;
            var appUser = await ConvertClaimsPrincipalToUser(user);
            return await _movimientoService.AddIngresoDeNominaAsync(appUser, ingresoDeNomina);
        }

        /// <summary>
        /// Crea un nuevo pago con tarjeta para el usuario autenticado.
        /// </summary>
        /// <param name="pagoConTarjeta">Objeto con los datos del pago con tarjeta a crear.</param>
        /// <returns>El objeto del pago con tarjeta creado.</returns>
        [Authorize]
        [HttpPost("Pagotarjeta/")]
        public async Task<ActionResult<Movimiento>> AddPagoConTarjeta([FromBody] PagoConTarjeta pagoConTarjeta)
        {
            _logger.LogInformation("Creating new pago con tarjeta");
            var user = _httpContextAccessor.HttpContext!.User;
            var appUser = await ConvertClaimsPrincipalToUser(user);
            return await _movimientoService.AddPagoConTarjetaAsync(appUser, pagoConTarjeta);
        }

        /// <summary>
        /// Revoca una transferencia para el usuario autenticado.
        /// </summary>
        /// <param name="transfGuid">El GUID de la transferencia a revocar.</param>
        /// <returns>El objeto de la transferencia revocada.</returns>
        [Authorize]
        [HttpDelete("Transferencia/{transfGuid}")]
        public async Task<ActionResult<Movimiento>> RevocarTransferencia(string transfGuid)
        {
            _logger.LogInformation("Revoking transferencia");
            var user = _httpContextAccessor.HttpContext!.User;
            var appUser = await ConvertClaimsPrincipalToUser(user);
            return await _movimientoService.RevocarTransferencia(appUser, transfGuid);
        }

        /// <summary>
        /// Convierte un objeto ClaimsPrincipal a un objeto User.
        /// </summary>
        /// <param name="user">El objeto ClaimsPrincipal del usuario autenticado.</param>
        /// <returns>Un objeto User con los datos del usuario.</returns>
        private async Task<User> ConvertClaimsPrincipalToUser(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return await _userRepository.GetByIdAsync(id);
        }
    }
}