using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services.Movimientos;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace VivesBankApi.Rest.Movimientos.Controller;

[ApiController]
[Route("api/[controller]")]
public class MovimientoController : ControllerBase
{
    private readonly IMovimientoService _movimientoService;
    private readonly IUserRepository _userService;
    private readonly ILogger<MovimientoController> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public MovimientoController(IMovimientoService movimientoService, 
        IUserRepository userService, 
        ILogger<MovimientoController> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _movimientoService = movimientoService;
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Domiciliacion>> CreateDomiciliacion([FromBody] Domiciliacion domiciliacion)
    {
        _logger.LogInformation("Creating new domiciliacion");
        var user = _httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await _movimientoService.AddDomiciliacionAsync(appUser, domiciliacion);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Movimiento>> AddTransferencia([FromBody] Transferencia transferencia)
    {
        _logger.LogInformation("Creating new transferencia");
        var user = _httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await _movimientoService.AddTransferenciaAsync(appUser, transferencia);
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Movimiento>> AddIngresoDeNomina([FromBody] IngresoDeNomina ingresoDeNomina)
    {
        _logger.LogInformation("Creating new ingreso de nomina");
        var user = _httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await _movimientoService.AddIngresoDeNominaAsync(appUser, ingresoDeNomina);
    }
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Movimiento>> AddPagoConTarjeta([FromBody] PagoConTarjeta pagoConTarjeta)
    {
        _logger.LogInformation("Creating new pago con tarjeta");
        var user = _httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await _movimientoService.AddPagoConTarjetaAsync(appUser, pagoConTarjeta);
    }
    
    [Authorize]
    [HttpPost("{transfGuid}")]
    public async Task<ActionResult<Movimiento>> RevocarTransferencia(string transfGuid)
    {
        _logger.LogInformation("Revoking transferencia");
        var user = _httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await _movimientoService.RevocarTransferencia(appUser, transfGuid);
    }
    
    
    private async Task<User> ConvertClaimsPrincipalToUser(ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await _userService.GetByIdAsync(id);
    }
}