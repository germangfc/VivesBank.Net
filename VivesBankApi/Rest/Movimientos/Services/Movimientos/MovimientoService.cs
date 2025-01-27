using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Validators;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public class MovimientoService(
    IMovimientoRepository movimientoRepository, 
    IDomiciliacionRepository domiciliacionRepository,
    IUserService userService,
    IClientService clientService,
    IAccountsService accountsService,
    ICreditCardService creditCardService,
    ILogger<MovimientoService> logger, 
    IOptions<ApiConfig> apiConfig,
    IWebsocketHandler websocketHandler,
    IHttpContextAccessor httpContextAccessor)
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

    [Authorize]
    public async Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion)
    {
        logger.LogInformation($"Adding domiciliacion cantidad: {domiciliacion.Cantidad}");
        
        // Validar que la cantidad es mayor que cero
        if (domiciliacion.Cantidad <= 0) throw new DomiciliacionInvalidCuantityException(domiciliacion.Id!, domiciliacion.Cantidad);
        
        // validar Iban correcto
        if (!IbanValidator.ValidateIban(domiciliacion.IbanDestino)) throw new InvalidDestinationIbanException(domiciliacion.IbanDestino);
        if (!IbanValidator.ValidateIban(domiciliacion.IbanOrigen)) throw new InvalidSourceIbanException(domiciliacion.IbanOrigen);

        // Validar que el cliente existe
        var client = await clientService.GetClientByIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        
        // Validar que la cuenta del cliente existe (origen)
        var account = await accountsService.GetAccountByIbanAsync(domiciliacion.IbanOrigen);
        if (account is null) throw new AccountsExceptions.AccountNotFoundByIban(domiciliacion.IbanOrigen);

        // Validar que la cuenta es de ese cliente
        if (!account.clientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(domiciliacion.IbanOrigen);
        
        // Validar si la domiciliación ya existe (mismo cobrador, cuenta destino)
        var clientDomiciliacion = await domiciliacionRepository.FindByClientGuid(client.Id);
        if (clientDomiciliacion.Any(d => d.IbanDestino == domiciliacion.IbanDestino)) throw new DuplicatedDomiciliacionException(domiciliacion.IbanDestino);
    
        // Guardar la domiciliación
        domiciliacion.UltimaEjecucion = DateTime.Now;
        domiciliacion.ClienteGuid = client.Id;
    
        // Notificación al cliente
         await EnviarNotificacionCreacionAsync(user, domiciliacion);
        // Retornar respuesta
        return await domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);

    }

    public async Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina)
    {
        logger.LogInformation("Adding new Ingreso de Nomina");
        // Validar que el ingreso de nomina es > 0
        if (ingresoDeNomina.Cantidad <= 0) throw new IngresoNominaInvalidCuantityException(ingresoDeNomina.Cantidad);
        
        // Validar Iban correcto
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanDestino)) throw new InvalidDestinationIbanException(ingresoDeNomina.IbanDestino);
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanOrigen)) throw new InvalidSourceIbanException(ingresoDeNomina.IbanOrigen);

        // Validar Cif
        if (!CifValidator.ValidateCif(ingresoDeNomina.CifEmpresa)) throw new InvalidCifException(ingresoDeNomina.CifEmpresa);

        // Validar que el cliente existe
        var client = await clientService.GetClientByIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        
        // Validar que la cuenta del cliente existe (destino)
        var clientAccount = await accountsService.GetCompleteAccountByIbanAsync(ingresoDeNomina.IbanDestino);
        if (clientAccount is null) throw new AccountsExceptions.AccountNotFoundByIban(ingresoDeNomina.IbanDestino);

        // Validar que la cuenta es de ese cliente
        if (!clientAccount.clientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(ingresoDeNomina.IbanDestino);

        // sumar al cliente la cantidad de la nomina
        clientAccount.Balance += ingresoDeNomina.Cantidad;
        //accountsService.UpdateAccountAsync(clientAccount.Id, UpdateAccountRequest);
        
        // Crear el movimiento
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            IngresoDeNomina = ingresoDeNomina
        };
        
        // Guardar el movimiento
        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);
        
        // Notificar al cliente
        EnviarNotificacionCreacionAsync(user, movimientoSaved);
        // Devolver el movimiento guardado
        return movimientoSaved;
    }

    public async Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta)
    {
        
        // Validar que la cantidad es mayor que cero
        if (pagoConTarjeta.Cantidad <= 0) throw new PagoTarjetaInvalidCuantityException(pagoConTarjeta.Cantidad);

        // Validar número tarjeta
        if (!NumTarjetaValidator.ValidateTarjeta(pagoConTarjeta.NumeroTarjeta)) throw new InvalidCardNumberException(pagoConTarjeta.NumeroTarjeta);

        // Validar que el cliente existe
        var client = await clientService.GetClientByIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);

        // Validar que la tarjeta existe
        //var clientCard = creditCardService.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta);
        
        // Validar que el cliente tiene esa tarjeta asociada a alguna de sus cuentas
        var clientAccounts = await accountsService.GetCompleteAccountByClientIdAsync(client.Id);
        if (clientAccounts is null) throw new AccountNotFoundByClientId(client.Id); // cliente no tiene cuentas asociadas

        // buscamos la cuenta a la que está asociada la tarjeta
        var cardAccount = clientAccounts.FirstOrDefault(a => a.TarjetaId == pagoConTarjeta.NumeroTarjeta);
        if (cardAccount is null) throw new CreditCardException.CreditCardNotFoundException(pagoConTarjeta.NumeroTarjeta); // tarjeta no está asociada a ninguna cuenta

        // Validar saldo suficiente en la cuenta
        var newBalance = cardAccount.Balance - pagoConTarjeta.Cantidad; 
        if ( newBalance < 0 ) throw new PagoTarjetaAccountInsufficientBalance(pagoConTarjeta.NumeroTarjeta);

        // restar al cliente
        cardAccount.Balance = newBalance;
//        await accountsService.UpdateAccountAsync(cardAccount.Id, new UpdateAccountRequest { Balance = cardAccount.Balance });

        // Crear el movimiento
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            PagoConTarjeta = pagoConTarjeta
        };

        // Guardar el movimiento
        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);

        // Notificar al cliente
        EnviarNotificacionCreacionAsync(user, movimientoSaved);
        // Retornar respuesta
        return movimientoSaved;

    }

    public Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia)
    {
        throw new NotImplementedException();
    }

    public Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid)
    {
        throw new NotImplementedException();
    }
    public async Task EnviarNotificacionCreacionAsync<T>(User user, T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Create.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };

        await websocketHandler.NotifyUserAsync(user.Id, notificacion);
    }

}