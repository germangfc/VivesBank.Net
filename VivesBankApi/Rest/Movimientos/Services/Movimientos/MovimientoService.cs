using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Clients.Models;
using VivesBankApi.Rest.Clients.Service;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;
using VivesBankApi.Rest.Movimientos.Repositories.Movimientos;
using VivesBankApi.Rest.Movimientos.Validators;
using VivesBankApi.Rest.Product.BankAccounts.Dto;
using VivesBankApi.Rest.Product.BankAccounts.Mappers;
using VivesBankApi.Rest.Product.BankAccounts.Services;
using VivesBankApi.Rest.Product.CreditCard.Exceptions;
using VivesBankApi.Rest.Product.CreditCard.Service;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using VivesBankApi.Utils.ApiConfig;
using VivesBankApi.Utils.GenericStorage.JSON;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;
using JsonConvert = Newtonsoft.Json.JsonConvert;

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
    IConnectionMultiplexer connection)
    : GenericStorageJson<Movimiento>(logger), IMovimientoService
{
    private readonly IDatabase _cache = connection.GetDatabase();
    public async Task<List<Movimiento>> FindAllMovimientosAsync()
    {
        logger.LogInformation("Finding all movimientos");
        return await movimientoRepository.GetAllMovimientosAsync();
    }

    public async Task<Movimiento> FindMovimientoByIdAsync(String id)
    {
        logger.LogInformation($"Finding movimiento by id: {id}");
        var movimiento = await GetByIdAsync(id);
        if (movimiento is null) throw new MovimientoNotFoundException(id);
        return movimiento;
    }

    public async Task<Movimiento> FindMovimientoByGuidAsync(string guid)
    {
        logger.LogInformation($"Finding movimiento by guid: {guid}");
        var movimiento = await GetByGuidAsync(guid);
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
        Movimiento? movimientoToUpdate = await GetByIdAsync(id) ?? throw new MovimientoNotFoundException(id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Guid);
        await _cache.StringSetAsync(id, JsonConvert.SerializeObject(movimientoToUpdate), TimeSpan.FromMinutes(10));
        return await movimientoRepository.UpdateMovimientoAsync(id, movimiento);
    }

    public async Task<Movimiento> DeleteMovimientoAsync(String id)
    {
        logger.LogInformation($"Deleting movimiento with id: {id}");
        Movimiento? movimientoToUpdate = await GetByIdAsync(id) ?? throw new MovimientoNotFoundException(id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Guid);
        var deletedMovimiento = await movimientoRepository.DeleteMovimientoAsync(id);
        return deletedMovimiento;
    }

    [Authorize]
    public async Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion)
    {
        logger.LogInformation($"Adding domiciliacion {domiciliacion}");
        
        // Validar que la cantidad es mayor que cero
        if (domiciliacion.Cantidad <= 0) throw new DomiciliacionInvalidAmountException(domiciliacion.Id!, domiciliacion.Cantidad);
        
        // validar Iban correcto
        if (!IbanValidator.ValidateIban(domiciliacion.IbanDestino)) throw new InvalidDestinationIbanException(domiciliacion.IbanDestino);
        if (!IbanValidator.ValidateIban(domiciliacion.IbanOrigen)) throw new InvalidSourceIbanException(domiciliacion.IbanOrigen);

        // Validar que el cliente existe
        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        
        // Validar que la cuenta del cliente existe (origen)
        var account = await accountsService.GetAccountByIbanAsync(domiciliacion.IbanOrigen);
        if (account is null) throw new AccountsExceptions.AccountNotFoundByIban(domiciliacion.IbanOrigen);

        // Validar que la cuenta es de ese cliente
        if (!account.clientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(domiciliacion.IbanOrigen);
        
        // Validar si la domiciliación ya existe (mismo cobrador (cuenta destino), mismo cliente (cuenta origen))
        logger.LogDebug($"Getting active direct debits for client client.id {client.Id}, domiciliacion.clientGuid: {domiciliacion.ClienteGuid}, user.id = {user.Id}");
        var clientDomiciliacion = await domiciliacionRepository.GetDomiciliacionesActivasByClienteGiudAsync(client.Id);
        if (clientDomiciliacion.Any(d => d.IbanDestino == domiciliacion.IbanDestino && d.IbanOrigen == domiciliacion.IbanOrigen)) 
            throw new DuplicatedDomiciliacionException(domiciliacion.IbanDestino);
    
        // Guardar la domiciliación
        domiciliacion.UltimaEjecucion = DateTime.UtcNow;
        domiciliacion.ClienteGuid = client.Id;
    
        // Notificación al cliente
         await EnviarNotificacionCreacionAsync(user, domiciliacion);
        // Retornar respuesta
        return await domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);

    }

    public async Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina)
    {
        logger.LogInformation($"Adding new Payroll Income {ingresoDeNomina} User.id {user.Id}");
        
        // Validar que el ingreso de nomina es > 0
        if (ingresoDeNomina.Cantidad <= 0) throw new IngresoNominaInvalidAmountException(ingresoDeNomina.Cantidad);
        
        // Validar Iban correcto
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanDestino)) throw new InvalidDestinationIbanException(ingresoDeNomina.IbanDestino);
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanOrigen)) throw new InvalidSourceIbanException(ingresoDeNomina.IbanOrigen);

        // Validar Cif
        if (!CifValidator.ValidateCif(ingresoDeNomina.CifEmpresa)) throw new InvalidCifException(ingresoDeNomina.CifEmpresa);

        // Validar que el cliente existe
        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        
        // Validar que la cuenta del cliente existe (destino)
        var clientAccount = await accountsService.GetCompleteAccountByIbanAsync(ingresoDeNomina.IbanDestino);
        if (clientAccount is null) throw new AccountsExceptions.AccountNotFoundByIban(ingresoDeNomina.IbanDestino);

        // Validar que la cuenta es de ese cliente
        if (!clientAccount.ClientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(ingresoDeNomina.IbanDestino);

        // sumar al cliente la cantidad de la nómina
        var newBalance = clientAccount.Balance + ingresoDeNomina.Cantidad;
        
        var updateAccountRequest = clientAccount.toUpdateAccountRequest();
        updateAccountRequest.Balance = newBalance;
        
        var updatedAccount = await accountsService.UpdateAccountAsync(clientAccount.Id, updateAccountRequest);
        logger.LogInformation($"New balance after Payroll Income: {updatedAccount.Balance}");

        // Crear el movimiento
        var now = DateTime.UtcNow;
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            IngresoDeNomina = ingresoDeNomina,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        // Guardar el movimiento
        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);
        
        // Notificar al cliente
        await EnviarNotificacionCreacionAsync(user, movimientoSaved);
        // Devolver el movimiento guardado
        return movimientoSaved;
    }

    public async Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta)
    {
        logger.LogInformation($"Adding new Credit Card Payment {pagoConTarjeta}");
        
        // Validar que la cantidad es mayor que cero
        if (pagoConTarjeta.Cantidad <= 0) throw new PagoTarjetaInvalidAmountException(pagoConTarjeta.Cantidad);

        // Validar número tarjeta
        if (!NumTarjetaValidator.ValidateTarjeta(pagoConTarjeta.NumeroTarjeta)) throw new InvalidCardNumberException(pagoConTarjeta.NumeroTarjeta);

        // Validar que el cliente existe
        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);

        // Validar que la tarjeta existe
        var clientCard = await creditCardService.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta);
        if (clientCard is null) throw new PagoTarjetaCreditCardNotFoundException(pagoConTarjeta.NumeroTarjeta);
        
        // Validar que el cliente tiene esa tarjeta asociada a alguna de sus cuentas
        var clientAccounts = await accountsService.GetCompleteAccountByClientIdAsync(client.Id);
        if (clientAccounts is null) throw new AccountNotFoundByClientIdException(client.Id); // cliente no tiene cuentas asociadas

        // buscamos la cuenta a la que está asociada la tarjeta
        var cardAccount = clientAccounts.FirstOrDefault(a => a.TarjetaId == clientCard.Id);
        if (cardAccount is null) throw new CreditCardException.CreditCardNotAssignedException(pagoConTarjeta.NumeroTarjeta); // tarjeta no está asociada a ninguna cuenta

        // Validar saldo suficiente en la cuenta
        var newBalance = cardAccount.Balance - pagoConTarjeta.Cantidad; 
        if ( newBalance < 0 ) throw new PagoTarjetaAccountInsufficientBalanceException(pagoConTarjeta.NumeroTarjeta);

        // restar al cliente
        var updateAccountRequest = cardAccount.toUpdateAccountRequest();
        updateAccountRequest.Balance = newBalance;
        
        var updatedAccount = await accountsService.UpdateAccountAsync(cardAccount.Id, updateAccountRequest);
        logger.LogInformation($"New balance after Credit Card Payment: {updatedAccount.Balance}");

        // Crear el movimiento
        var now = DateTime.UtcNow;
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            PagoConTarjeta = pagoConTarjeta,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        // Guardar el movimiento
        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);

        // Notificar al cliente
        await EnviarNotificacionCreacionAsync(user, movimientoSaved);
        // Retornar respuesta
        return movimientoSaved;

    }

    public async Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia)
    {
        logger.LogInformation("Adding new transfer");
        
        // Validar que las cuentas de origen y destino sean distintas
        if (transferencia.IbanOrigen.Equals(transferencia.IbanDestino)) throw new TransferSameIbanException(transferencia.IbanOrigen);

        // Validar que la cantidad es mayor que cero
        if (transferencia.Cantidad <= 0) throw new TransferInvalidAmountException(transferencia.Cantidad);
        
        // Validar Iban correcto
        if (!IbanValidator.ValidateIban(transferencia.IbanDestino)) throw new InvalidDestinationIbanException(transferencia.IbanDestino);
        if (!IbanValidator.ValidateIban(transferencia.IbanOrigen)) throw new InvalidSourceIbanException(transferencia.IbanOrigen);

        // Validar que el cliente existe
        var originClient = await clientService.GetClientByUserIdAsync(user.Id);
        if (originClient is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        
        // Validar que la cuenta origen existe
        var originAccount = await accountsService.GetCompleteAccountByIbanAsync(transferencia.IbanOrigen);
        if (originAccount is null) throw new AccountsExceptions.AccountNotFoundByIban(transferencia.IbanOrigen);

        // Validar que la cuenta es de ese cliente
        if (!originAccount.ClientID.Equals(originClient.Id)) throw new AccountsExceptions.AccountUnknownIban(transferencia.IbanOrigen);

        // Validar saldo suficiente en cuenta origen
        var newBalanceOrigin = originAccount.Balance - transferencia.Cantidad; 
        if (newBalanceOrigin < 0) throw new TransferInsufficientBalance(transferencia.IbanOrigen);

        // Movimiento auxiliar para poder usarlo fuera del try-catch
        // Se asocia posteriormente al movimiento origen para gestionar una posible revocación de la transferencia
        Movimiento destinationMovementAux = new Movimiento();
        try
        { 
            // Validar que la cuenta destino existe
            var destinationAccount = await accountsService.GetCompleteAccountByIbanAsync(transferencia.IbanDestino);
            
            // La cuenta destino se actualiza solo si es de un cliente del banco
            if (destinationAccount != null)
            {
                // Sumar a la cuenta destino
                var newBalanceDestination = destinationAccount.Balance + transferencia.Cantidad;
                var updateAccountRequestDestination = destinationAccount.toUpdateAccountRequest();
                updateAccountRequestDestination.Balance = newBalanceDestination;
                var updatedAccountDestination = await accountsService.UpdateAccountAsync(destinationAccount.Id, updateAccountRequestDestination);
                logger.LogInformation($"New balance destination account after Transfer: {updatedAccountDestination.Balance}");

                // Crear el movimiento del ingreso al cliente destino
                logger.LogInformation("Creating destination movement");
                var nowDest = DateTime.UtcNow;
                Movimiento newDestinationMovement = new Movimiento
                {
                    ClienteGuid = destinationAccount.ClientID,
                    Transferencia = transferencia,
                    CreatedAt = nowDest,
                    UpdatedAt = nowDest,
                    IsDeleted = false
                };
                
                // Guardar el movimiento destino
                logger.LogInformation("Saving destination movement");
                await movimientoRepository.AddMovimientoAsync(newDestinationMovement);
                
                // Notificar al cliente destino
                var destinationClient = await clientService.GetClientByIdAsync(destinationAccount.ClientID);
                if (destinationClient is null) throw new ClientExceptions.ClientNotFoundException(destinationAccount.ClientID);
                var destinationUser = await userService.GetUserByIdAsync(destinationClient.UserId);
                if (destinationUser is null) throw new UserNotFoundException(destinationClient.UserId);

                await EnviarNotificacionCreacionAsync(destinationUser.ToUser(), newDestinationMovement);

                // sacar el movimiento destino al auxiliar
                destinationMovementAux = newDestinationMovement;
            }
        }
        catch (AccountsExceptions.AccountNotFoundByIban ex)
        {
            logger.LogError($"{ex.Message}");
        }
        
        // Restar de la cuenta origen
         var updateAccountRequestOrigin = originAccount.toUpdateAccountRequest();
         updateAccountRequestOrigin.Balance = newBalanceOrigin;
         var updatedAccountOrigin = await accountsService.UpdateAccountAsync(originAccount.Id, updateAccountRequestOrigin);
         logger.LogInformation($"New balance origin account after Transfer: {updatedAccountOrigin.Balance}");

        // Crear el movimiento al cliente origen
        logger.LogInformation("Creating origin movement");
        var now = DateTime.UtcNow;
        Movimiento newOriginMovement = new Movimiento
        {
            ClienteGuid = originClient.Id,
            Transferencia = new Transferencia
            {
                IbanOrigen = transferencia.IbanOrigen,
                IbanDestino = transferencia.IbanDestino,
                Cantidad = decimal.Negate(transferencia.Cantidad),
                NombreBeneficiario = transferencia.NombreBeneficiario,
                MovimientoDestino = destinationMovementAux.Id ?? null
            },
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        // Guardar el movimiento origen
        var originSavedMovement = await movimientoRepository.AddMovimientoAsync(newOriginMovement);

        // Notificar al cliente origen
        logger.LogInformation("Notifying transfer origin client");
        await EnviarNotificacionCreacionAsync(user, newOriginMovement);
        
        // Retornar respuesta
        return originSavedMovement;
    }

    public async Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid)
    {
        logger.LogInformation($"Revoking Transfer Id: {movimientoTransferenciaGuid}, user: {user.Id}");
        
        // Obtener el movimiento original
        var originalMovement = await movimientoRepository.GetMovimientoByGuidAsync(movimientoTransferenciaGuid);
        if (originalMovement is null) throw new MovimientoNotFoundException(movimientoTransferenciaGuid);
        
        // validar que no haya pasado 1 día 
        var dateOriginalMovement = originalMovement.CreatedAt;
        if (dateOriginalMovement.HasValue && (DateTime.UtcNow - dateOriginalMovement.Value).TotalHours > 24) 
            throw new NotRevocableMovimientoException(movimientoTransferenciaGuid);

        // Verificar que el movimiento es una transferencia
        if (originalMovement.Transferencia is null) throw new MovementIsNotTransferException(movimientoTransferenciaGuid);

        // Verificar que el usuario que solicita la revocación existe y es el propietario de la cuenta de origen
        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);
        logger.LogInformation($"client.Id = {client.Id}, originalmovement.clientGuid = {originalMovement.ClienteGuid}");
        if (!client.Id.Equals(originalMovement.ClienteGuid))
            throw new AccountsExceptions.AccountUnknownIban(originalMovement.Transferencia.IbanOrigen);

        // Cantidad a restaurar
        var transferAmount = originalMovement.Transferencia.Cantidad;

        // Obtener las cuentas involucradas
        var originAccount = await accountsService.GetCompleteAccountByIbanAsync(originalMovement.Transferencia.IbanOrigen);
        if (originAccount is null) throw new AccountsExceptions.AccountNotFoundByIban(originalMovement.Transferencia.IbanOrigen);

        // Revertir la transferencia

        //    Sumar a la cuenta origen
        var newBalanceOrigin = originAccount.Balance - transferAmount; 
        
        var updateAccountRequestOrigin = originAccount.toUpdateAccountRequest();
        updateAccountRequestOrigin.Balance = newBalanceOrigin;
        var updatedAccountOrigin = await accountsService.UpdateAccountAsync(originAccount.Id, updateAccountRequestOrigin);
        logger.LogInformation($"New balance origin account after Transfer Revoking: {updatedAccountOrigin.Balance}");

        //    Notificar la revocación de la transferencia
        await EnviarNotificacionDeleteAsync(user, originalMovement);
        
        //    Restar de la cuenta destino si era de nuestro banco
        try {
            var destinationAccount = await accountsService.GetCompleteAccountByIbanAsync(originalMovement.Transferencia.IbanDestino);

            var newBalanceDestination = destinationAccount.Balance + transferAmount;
            var updateAccountRequestDestination = destinationAccount.toUpdateAccountRequest();
            updateAccountRequestDestination.Balance = newBalanceDestination;
            var updatedAccountDestination = await accountsService.UpdateAccountAsync(destinationAccount.Id, updateAccountRequestDestination);
            logger.LogInformation($"New balance destination account after Transfer Revoking: {updatedAccountDestination.Balance}");
            
            // Notificar al cliente destino
            var destinationClient = await clientService.GetClientByIdAsync(destinationAccount.ClientID);
            if (destinationClient is null) throw new ClientExceptions.ClientNotFoundException(destinationAccount.ClientID);
            var destinationUser = await userService.GetUserByIdAsync(destinationClient.UserId);
            if (destinationUser is null) throw new UserNotFoundException(destinationClient.UserId);

            await EnviarNotificacionCreacionAsync(destinationUser.ToUser(), originalMovement);
            
        } catch (AccountsExceptions.AccountNotFoundByIban ex)
        { 
            logger.LogInformation($"Claim amount to external bank ({ex.Message})");
        }

        // Marcar ambos movimientos como eliminados, simplemente se anulan, no se crean nuevos movimientos de revocación
        originalMovement.IsDeleted = true;
        if (originalMovement.Id != null)
        {
            await movimientoRepository.UpdateMovimientoAsync(originalMovement.Id, originalMovement);
            logger.LogInformation("Revoked original movement");

            // Verificamos si hay un movimiento destino
            if (originalMovement.Transferencia.MovimientoDestino != null)
            {
                var originalDestinationMovement =
                    await movimientoRepository.GetMovimientoByIdAsync(originalMovement.Transferencia.MovimientoDestino);

                // Si existe el movimiento destino, lo marcamos como eliminado
                if (originalDestinationMovement != null)
                {
                    originalDestinationMovement.IsDeleted = true;
                    if (originalDestinationMovement.Id != null)
                    {
                        await movimientoRepository.UpdateMovimientoAsync(originalDestinationMovement.Id, originalDestinationMovement);
                        logger.LogInformation("Revoked destination movement from original movement");
                    }
                }
                else logger.LogInformation("Destination movement not found");
            }
            else logger.LogInformation("There was no destination movement in original movement");
        }

        // Retornar respuesta
        return originalMovement;
    }
    
    // CACHE
    
    private async Task<Movimiento?> GetByIdAsync(string id)
    {
        var cachedMovimientos = await _cache.StringGetAsync(id);
        if (!cachedMovimientos.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Movimiento>(cachedMovimientos);
        }
        Movimiento? movimiento = await movimientoRepository.GetMovimientoByIdAsync(id);
        if (movimiento != null)
        {
            await _cache.StringSetAsync("movimiento:" + id, JsonConvert.SerializeObject(movimiento), TimeSpan.FromMinutes(10));
            return movimiento;
        }
        return null;
    }
    
    
    private async Task<Movimiento?> GetByGuidAsync(string id)
    {
        var cachedMovimientos = await _cache.StringGetAsync(id);
        if (!cachedMovimientos.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<Movimiento>(cachedMovimientos);
        }
        Movimiento? movimiento = await movimientoRepository.GetMovimientoByGuidAsync(id);
        if (movimiento != null)
        {
            await _cache.StringSetAsync("movimiento:" + id, JsonConvert.SerializeObject(movimiento), TimeSpan.FromMinutes(10));
            return movimiento;
        }
        return null;
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

    public async Task EnviarNotificacionDeleteAsync<T>(User user, T t)
    {
        var notificacion = new Notification<T>
        {
            Type = Notification<T>.NotificationType.Delete.ToString(),
            CreatedAt = DateTime.Now,
            Data = t
        };

        await websocketHandler.NotifyUserAsync(user.Id, notificacion);
    }
}