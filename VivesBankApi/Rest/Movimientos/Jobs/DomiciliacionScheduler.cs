using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.Rest.Movimientos.Jobs;

using Quartz;

public class DomiciliacionScheduler : IJob
{
    private readonly IDomiciliacionRepository _domiciliacionRepository;
    private readonly IMovimientoRepository _movimientoRepository;
    private readonly IAccountsService _accountsService;
    private readonly IUserService _userService;
    private readonly IClientService _clientService;
    private readonly ILogger<DomiciliacionScheduler> _logger;
    private readonly IWebsocketHandler _websocketHandler;
    public IServiceProvider ServiceProvider { get; set; }

    public DomiciliacionScheduler(
        IDomiciliacionRepository domiciliacionRepository,
        IMovimientoRepository movimientoRepository,
        IAccountsService accountsService,
        IUserService userService,
        IClientService clientService,
        ILogger<DomiciliacionScheduler> logger,
        IWebsocketHandler websocketHandler)
    {
        _domiciliacionRepository = domiciliacionRepository;
        _movimientoRepository = movimientoRepository;
        _accountsService = accountsService;
        _userService = userService;
        _clientService = clientService;
        _logger = logger;
        _websocketHandler = websocketHandler;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Processing scheduled direct debits (domiciliaciones)");
        // Asignar el ServiceProvider desde el JobDataMap
        ServiceProvider = context.JobDetail.JobDataMap["ServiceProvider"] as IServiceProvider;

        using (var scope = ServiceProvider.CreateScope())
        {
            // Filtrar domiciliaciones activas que requieren ejecución
            var domiciliaciones = (await _domiciliacionRepository.GetAllDomiciliacionesActivasAsync())
                .Where(d => RequiereEjecucion(d, DateTime.Now))
                .ToList();

            _logger.LogInformation($"Número de domiciliaciones activas: {domiciliaciones.Count}");

            foreach (var domiciliacion in domiciliaciones)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var originAccount = await _accountsService.GetCompleteAccountByIbanAsync(domiciliacion.IbanOrigen);
                    if (originAccount == null)
                        throw new AccountsExceptions.AccountNotFoundByIban(domiciliacion.IbanOrigen);
                    await EjecutarDomiciliacionAsync(domiciliacion, originAccount, now);
                    domiciliacion.UltimaEjecucion = now;
                    await _domiciliacionRepository.UpdateDomiciliacionAsync(domiciliacion.Id, domiciliacion);
                }
                catch (DomiciliacionAccountInsufficientBalanceException)
                {
                    _logger.LogWarning(
                        $"Insufficient balance for Client: {domiciliacion.ClienteGuid}, Account: {domiciliacion.IbanOrigen}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Error processing direct debit: {domiciliacion.Guid}, Account: {domiciliacion.IbanOrigen}: {ex.Message}");
                }
            }
        }

    }


    private async Task EjecutarDomiciliacionAsync(Domiciliacion domiciliacion, AccountCompleteResponse originAccount, DateTime date)
    {
        _logger.LogInformation($"Executing direct debit Client: {domiciliacion.ClienteGuid}, Company: {domiciliacion.NombreAcreedor}, Quantity: {domiciliacion.Cantidad} on account: {originAccount.IBAN}");

        // Comprobación saldo suficiente
        if (originAccount.Balance < domiciliacion.Cantidad) throw new DomiciliacionAccountInsufficientBalanceException(domiciliacion.IbanOrigen);

        // Restamos del saldo y actualizamos la cuenta
        var newBalanceOrigin = originAccount.Balance - domiciliacion.Cantidad; 
        var updateAccountRequestOrigin = originAccount.toUpdateAccountRequest();
        updateAccountRequestOrigin.Balance = newBalanceOrigin;
        var updatedAccountOrigin = await _accountsService.UpdateAccountAsync(originAccount.Id, updateAccountRequestOrigin);
        _logger.LogInformation($"New balance origin account after direct debit execution: {updatedAccountOrigin.Balance}");

        // Registrar el movimiento
        var movimiento = new Movimiento
        {
            ClienteGuid = originAccount.ClientID,
            Domiciliacion = domiciliacion,
            CreatedAt = date,
            UpdatedAt = date,
            IsDeleted = false
        };
        
        _logger.LogInformation("Saving direct debit movement");
        await _movimientoRepository.AddMovimientoAsync(movimiento);

        // Notificar la domiciliación
        var cliente = await _clientService.GetClientByIdAsync(domiciliacion.ClienteGuid);
        if (cliente is null) throw new ClientExceptions.ClientNotFoundException(domiciliacion.ClienteGuid);

        var user = await _userService.GetUserByIdAsync(cliente.UserId);
        await EnviarNotificacionExecuteAsync(user, movimiento);
        
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
    
    public async Task EnviarNotificacionExecuteAsync<T>(UserResponse userResponse,T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Execute.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };
        await _websocketHandler.NotifyUserAsync(userResponse.Id, notificacion);
    }
}