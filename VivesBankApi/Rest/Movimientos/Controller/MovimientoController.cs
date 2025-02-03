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
public class MovimientoController(
    IMovimientoService movimientoService,
    IUserRepository userRepository,
    ILogger<MovimientoController> logger,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    [Authorize]
    [HttpPost("Domiciliacion/")]
    public async Task<ActionResult<Domiciliacion>> CreateDomiciliacion([FromBody] Domiciliacion domiciliacion)
    {
        logger.LogInformation("Creating new domiciliacion");
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddDomiciliacionAsync(appUser, domiciliacion);
    }
    
    [Authorize]
    [HttpPost("Transferencia/")]
    public async Task<ActionResult<Movimiento>> AddTransferencia([FromBody] Transferencia transferencia)
    {
        logger.LogInformation("Creating new transferencia");
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddTransferenciaAsync(appUser, transferencia);
    }
    
    [Authorize]
    [HttpPost("Ingresonomina/")]
    public async Task<ActionResult<Movimiento>> AddIngresoDeNomina([FromBody] IngresoDeNomina ingresoDeNomina)
    {
        logger.LogInformation("Creating new ingreso de nomina");
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddIngresoDeNominaAsync(appUser, ingresoDeNomina);
    }
    [Authorize]
    [HttpPost("Pagotarjeta/")]
    public async Task<ActionResult<Movimiento>> AddPagoConTarjeta([FromBody] PagoConTarjeta pagoConTarjeta)
    {
        logger.LogInformation("Creating new pago con tarjeta");
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddPagoConTarjetaAsync(appUser, pagoConTarjeta);
    }
    
    [Authorize]
    [HttpDelete("Transferencia/{transfGuid}")]
    public async Task<ActionResult<Movimiento>> RevocarTransferencia(string transfGuid)
    {
        logger.LogInformation("Revoking transferencia");
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.RevocarTransferenciaAsync(appUser, transfGuid);
    }
    
    
    private async Task<User> ConvertClaimsPrincipalToUser(ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return await userRepository.GetByIdAsync(id);
    }
}