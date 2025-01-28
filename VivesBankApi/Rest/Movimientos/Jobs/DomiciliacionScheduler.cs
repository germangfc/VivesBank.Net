using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Service;

namespace VivesBankApi.Rest.Movimientos.Jobs;

using Quartz;

public class DomiciliacionScheduler(
    IDomiciliacionRepository domiciliacionRepository,
    IMovimientoRepository movimientoRepository,
    IAccountsService accountService,
    IUserService userService,
    IClientService clientService,
    ILogger<DomiciliacionScheduler> logger
) :IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Processing scheduled direct debits (domiciliaciones)"); 
        
        // Filtrar domiciliaciones activas que requieren ejecución
        var domiciliaciones = (await domiciliacionRepository.GetAllDomiciliacionesActivasAsync())
            .Where(d => RequiereEjecucion(d, DateTime.Now))
            .ToList();

        // Lanzamos asíncronamente las actualizaciones. Foreach no las lanza asíncronamente y 
        // no actualiza una domiciliación hasta que no termine la anterior
        var tasks = new List<Task>();

        foreach (var domiciliacion in domiciliaciones)
        {
            // Esperamos a que se complete 'ejecutarDomiciliacion' antes de agregar la tarea de actualización
            await EjecutarDomiciliacion(domiciliacion);
            
            // Añadimos cada actualización de la domiciliación a la lista de tareas
            domiciliacion.UltimaEjecucion = DateTime.Now;
            tasks.Add(domiciliacionRepository.UpdateDomiciliacionAsync(domiciliacion.Id, domiciliacion));
        }

        // Esperamos a que todas las tareas de actualización de domiciliacion terminen para continuar 
        await Task.WhenAll(tasks);

    }

    private async Task EjecutarDomiciliacion(Domiciliacion domiciliacion)
    {
        logger.LogInformation($"Executing direct debit Client: {domiciliacion.ClienteGuid}, Company: {domiciliacion.NombreAcreedor}, Quantity: {domiciliacion.Cantidad}");

        // Obtener la cuenta donde se cargará la domiciliación
        var originAccount = await accountService.GetCompleteAccountByIbanAsync(domiciliacion.IbanOrigen);
        if (originAccount == null) throw new AccountsExceptions.AccountNotFoundByIban(domiciliacion.IbanOrigen);

        // Comprobación saldo suficiente
        if (originAccount.Balance < domiciliacion.Cantidad) throw new DomiciliacionAccountInsufficientBalanceException(domiciliacion.IbanOrigen);

        // Restamos del saldo y actualizamos la cuenta
        originAccount.Balance -= domiciliacion.Cantidad;
        //await accountService.UpdateAccountAsync(originAccount.Id, originAccount.toAccountRequestUpdate);
        
        // Registrar el movimiento
        var movimiento = new Movimiento
        {
            ClienteGuid = originAccount.clientID,
            Domiciliacion = domiciliacion
        };
        
        await movimientoRepository.AddMovimientoAsync(movimiento);

        // Notificar la domiciliación
        
    }
    private bool RequiereEjecucion(Domiciliacion domiciliacion, DateTime ahora)
    {
        switch (domiciliacion.Periodicidad)
        {
            case Periodicidad.DIARIA:  return domiciliacion.UltimaEjecucion.AddDays(1) < ahora;
            case Periodicidad.SEMANAL: return domiciliacion.UltimaEjecucion.AddDays(7) < ahora;
            case Periodicidad.MENSUAL: return domiciliacion.UltimaEjecucion.AddMonths(1) < ahora;
            case Periodicidad.ANUAL:   return domiciliacion.UltimaEjecucion.AddYears(1) < ahora;
            default: return false;                
        }
    }
}