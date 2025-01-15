using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Services;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace VivesBankApi.Rest.Movimientos.Resolver;

[ExtendObjectType(Name = "Mutation")]
public class MovimientosMutation(IMovimientoService movimientoService, IHttpContextAccessor httpContextAccessor, IUserRepository userService)
{
    [Authorize] 
    public async Task<Domiciliacion> AddDomiciliacion(Domiciliacion domiciliacion)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddDomiciliacionAsync(appUser, domiciliacion);
    }
    
    [Authorize] 
    public async Task<Movimiento> AddTransferencia(Transferencia transferencia)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddTransferenciaAsync(appUser, transferencia);
    }
    
    [Authorize] 
    public async Task<Movimiento> AddIngresoDeNomina(IngresoDeNomina ingresoDeNomina)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddIngresoDeNominaAsync(appUser, ingresoDeNomina);
    }
    
    [Authorize] 
    public async Task<Movimiento> AddPagoConTarjeta(PagoConTarjeta pagoConTarjeta)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.AddPagoConTarjetaAsync(appUser, pagoConTarjeta);
    }
    
    [Authorize] 
    public async Task<Movimiento> RevocarTransferencia(string movimientoTransferenciaGuid)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var appUser = await ConvertClaimsPrincipalToUser(user);
        return await movimientoService.RevocarTransferencia(appUser, movimientoTransferenciaGuid);
    }

    private async Task<User> ConvertClaimsPrincipalToUser(ClaimsPrincipal user)
    {
       var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
         return await userService.GetByIdAsync(int.Parse(id));
    }
}