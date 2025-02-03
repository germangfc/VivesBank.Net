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


/// <summary>
/// Servicio para gestionar movimientos financieros como transferencias, nóminas y pagos.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
/// <version>1.0.0</version>
public class MovimientoService(
    
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase MovimientoService con las dependencias necesarias.
    /// </summary>
    /// <param name="movimientoRepository">Repositorio para gestionar los movimientos</param>
    /// <param name="domiciliacionRepository">Repositorio para gestionar las domiciliaciones</param>
    /// <param name="userService">Servicio para la gestión de usuarios</param>
    /// <param name="clientService">Servicio para la gestión de clientes</param>
    /// <param name="accountsService">Servicio para la gestión de cuentas</param>
    /// <param name="creditCardService">Servicio para la gestión de tarjetas de crédito</param>
    /// <param name="logger">Logger para registrar eventos</param>
    /// <param name="apiConfig">Configuración de la API</param>
    /// <param name="websocketHandler">Manejador de conexiones WebSocket</param>
    /// <param name="connection">Conexión a la base de datos</param>
    /// <version>1.0.0</version>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
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
    
    /// <summary>
    /// Obtiene todos los movimientos registrados.
    /// </summary>
    /// <returns>Lista de movimientos</returns>
    /// <version>1.0.0</version>
    public async Task<List<Movimiento>> FindAllMovimientosAsync()
    {
        logger.LogInformation("Finding all movimientos");
        return await movimientoRepository.GetAllMovimientosAsync();
    }

    /// <summary>
    /// Obtiene un movimiento por su ID.
    /// </summary>
    /// <param name="id">ID del movimiento</param>
    /// <returns>Movimiento encontrado</returns>
    /// <exception cref="MovimientoNotFoundException">Lanzado cuando no se encuentra el movimiento</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> FindMovimientoByIdAsync(string id)
    {
        logger.LogInformation($"Finding movimiento by id: {id}");
        var movimiento = await GetByIdAsync(id);
        if (movimiento is null) throw new MovimientoNotFoundException(id);
        return movimiento;
    }

    /// <summary>
    /// Obtiene un movimiento por su GUID.
    /// </summary>
    /// <param name="guid">GUID del movimiento</param>
    /// <returns>Movimiento encontrado</returns>
    /// <exception cref="MovimientoNotFoundException">Lanzado cuando no se encuentra el movimiento</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> FindMovimientoByGuidAsync(string guid)
    {
        logger.LogInformation($"Finding movimiento by guid: {guid}");
        var movimiento = await GetByGuidAsync(guid);
        if (movimiento is null) throw new MovimientoNotFoundException(guid);
        return movimiento;
    }

    /// <summary>
    /// Obtiene todos los movimientos asociados a un cliente.
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Lista de movimientos asociados al cliente</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<List<Movimiento>> FindAllMovimientosByClientAsync(string clienteId)
    {
        logger.LogInformation($"Finding movimientos by client id: {clienteId}");
        return await movimientoRepository.GetMovimientosByClientAsync(clienteId);
    }

    /// <summary>
    /// Añade un nuevo movimiento.
    /// </summary>
    /// <param name="movimiento">Movimiento a agregar</param>
    /// <returns>Movimiento agregado</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> AddMovimientoAsync(Movimiento movimiento)
    {
        logger.LogInformation($"Adding movimiento: {movimiento}");
        return await movimientoRepository.AddMovimientoAsync(movimiento);
    }

    /// <summary>
    /// Actualiza un movimiento existente.
    /// </summary>
    /// <param name="id">ID del movimiento a actualizar</param>
    /// <param name="movimiento">Nuevo movimiento</param>
    /// <returns>Movimiento actualizado</returns>
    /// <exception cref="MovimientoNotFoundException">Lanzado cuando no se encuentra el movimiento a actualizar</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> UpdateMovimientoAsync(string id, Movimiento movimiento)
    {
        logger.LogInformation($"Updating movimiento with id: {id}");
        Movimiento? movimientoToUpdate = await GetByIdAsync(id) ?? throw new MovimientoNotFoundException(id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Guid);
        await _cache.StringSetAsync(id, JsonConvert.SerializeObject(movimientoToUpdate), TimeSpan.FromMinutes(10));
        return await movimientoRepository.UpdateMovimientoAsync(id, movimiento);
    }

    /// <summary>
    /// Elimina un movimiento existente.
    /// </summary>
    /// <param name="id">ID del movimiento a eliminar</param>
    /// <returns>Movimiento eliminado</returns>
    /// <exception cref="MovimientoNotFoundException">Lanzado cuando no se encuentra el movimiento a eliminar</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> DeleteMovimientoAsync(string id)
    {
        logger.LogInformation($"Deleting movimiento with id: {id}");
        Movimiento? movimientoToUpdate = await GetByIdAsync(id) ?? throw new MovimientoNotFoundException(id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Id);
        await _cache.KeyDeleteAsync(movimientoToUpdate.Guid);
        var deletedMovimiento = await movimientoRepository.DeleteMovimientoAsync(id);
        return deletedMovimiento;
    }

    /// <summary>
    /// Añade una nueva domiciliación.
    /// </summary>
    /// <param name="user">Usuario que realiza la acción</param>
    /// <param name="domiciliacion">Datos de la domiciliación</param>
    /// <returns>Domiciliación añadida</returns>
    /// <exception cref="DomiciliacionInvalidAmountException">Lanzado cuando la cantidad de la domiciliación es incorrecta</exception>
    /// <exception cref="InvalidDestinationIbanException">Lanzado cuando el IBAN de destino es incorrecto</exception>
    /// <exception cref="InvalidSourceIbanException">Lanzado cuando el IBAN de origen es incorrecto</exception>
    /// <exception cref="ClientNotFoundException">Lanzado cuando el cliente no se encuentra</exception>
    /// <exception cref="AccountNotFoundByIban">Lanzado cuando la cuenta de origen no se encuentra</exception>
    /// <exception cref="DuplicatedDomiciliacionException">Lanzado cuando ya existe una domiciliación duplicada</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    [Authorize]
    public async Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion)
    {
        logger.LogInformation($"Adding domiciliacion {domiciliacion}");

        if (domiciliacion.Cantidad <= 0) throw new DomiciliacionInvalidAmountException(domiciliacion.Id!, domiciliacion.Cantidad);
        if (!IbanValidator.ValidateIban(domiciliacion.IbanDestino)) throw new InvalidDestinationIbanException(domiciliacion.IbanDestino);
        if (!IbanValidator.ValidateIban(domiciliacion.IbanOrigen)) throw new InvalidSourceIbanException(domiciliacion.IbanOrigen);

        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);

        var account = await accountsService.GetAccountByIbanAsync(domiciliacion.IbanOrigen);
        if (account is null) throw new AccountsExceptions.AccountNotFoundByIban(domiciliacion.IbanOrigen);

        if (!account.clientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(domiciliacion.IbanOrigen);

        var clientDomiciliacion = await domiciliacionRepository.GetDomiciliacionesActivasByClienteGiudAsync(client.Id);
        if (clientDomiciliacion.Any(d => d.IbanDestino == domiciliacion.IbanDestino && d.IbanOrigen == domiciliacion.IbanOrigen)) 
            throw new DuplicatedDomiciliacionException(domiciliacion.IbanDestino);

        domiciliacion.UltimaEjecucion = DateTime.UtcNow;
        domiciliacion.ClienteGuid = client.Id;

        await EnviarNotificacionCreacionAsync(user, domiciliacion);
        return await domiciliacionRepository.AddDomiciliacionAsync(domiciliacion);
    }

    /// <summary>
    /// Añade un nuevo ingreso de nómina.
    /// </summary>
    /// <param name="user">Usuario que realiza la acción</param>
    /// <param name="ingresoDeNomina">Datos del ingreso de nómina</param>
    /// <returns>Movimiento de nómina creado</returns>
    /// <exception cref="IngresoNominaInvalidAmountException">Lanzado cuando la cantidad de la nómina es incorrecta</exception>
    /// <exception cref="InvalidDestinationIbanException">Lanzado cuando el IBAN de destino es incorrecto</exception>
    /// <exception cref="InvalidSourceIbanException">Lanzado cuando el IBAN de origen es incorrecto</exception>
    /// <exception cref="InvalidCifException">Lanzado cuando el CIF de la empresa es incorrecto</exception>
    /// <exception cref="ClientNotFoundException">Lanzado cuando el cliente no se encuentra</exception>
    /// <exception cref="AccountNotFoundByIban">Lanzado cuando la cuenta de destino no se encuentra</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina)
    {
        logger.LogInformation($"Adding new Payroll Income {ingresoDeNomina} User.id {user.Id}");

        if (ingresoDeNomina.Cantidad <= 0) throw new IngresoNominaInvalidAmountException(ingresoDeNomina.Cantidad);
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanDestino)) throw new InvalidDestinationIbanException(ingresoDeNomina.IbanDestino);
        if (!IbanValidator.ValidateIban(ingresoDeNomina.IbanOrigen)) throw new InvalidSourceIbanException(ingresoDeNomina.IbanOrigen);
        if (!CifValidator.ValidateCif(ingresoDeNomina.CifEmpresa)) throw new InvalidCifException(ingresoDeNomina.CifEmpresa);

        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);

        var clientAccount = await accountsService.GetCompleteAccountByIbanAsync(ingresoDeNomina.IbanDestino);
        if (clientAccount is null) throw new AccountsExceptions.AccountNotFoundByIban(ingresoDeNomina.IbanDestino);

        if (!clientAccount.ClientID.Equals(client.Id)) throw new AccountsExceptions.AccountUnknownIban(ingresoDeNomina.IbanDestino);

        var newBalance = clientAccount.Balance + ingresoDeNomina.Cantidad;
        var updateAccountRequest = clientAccount.toUpdateAccountRequest();
        updateAccountRequest.Balance = newBalance;

        var updatedAccount = await accountsService.UpdateAccountAsync(clientAccount.Id, updateAccountRequest);
        logger.LogInformation($"New balance after Payroll Income: {updatedAccount.Balance}");

        var now = DateTime.UtcNow;
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            IngresoDeNomina = ingresoDeNomina,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);

        await EnviarNotificacionCreacionAsync(user, movimientoSaved);
        return movimientoSaved;
    }

    /// <summary>
    /// Añade un pago con tarjeta.
    /// </summary>
    /// <param name="user">Usuario que realiza la acción</param>
    /// <param name="pagoConTarjeta">Datos del pago con tarjeta</param>
    /// <returns>Movimiento de pago con tarjeta creado</returns>
    /// <exception cref="PagoTarjetaInvalidAmountException">Lanzado cuando la cantidad del pago es incorrecta</exception>
    /// <exception cref="InvalidCardNumberException">Lanzado cuando el número de tarjeta es incorrecto</exception>
    /// <exception cref="ClientNotFoundException">Lanzado cuando el cliente no se encuentra</exception>
    /// <exception cref="CreditCardNotFoundException">Lanzado cuando la tarjeta no se encuentra</exception>
    /// <exception cref="CreditCardNotAssignedException">Lanzado cuando la tarjeta no está asociada a ninguna cuenta</exception>
    /// <exception cref="PagoTarjetaAccountInsufficientBalanceException">Lanzado cuando la cuenta no tiene saldo suficiente</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
    public async Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta)
    {
        logger.LogInformation($"Adding new Credit Card Payment {pagoConTarjeta}");

        if (pagoConTarjeta.Cantidad <= 0) throw new PagoTarjetaInvalidAmountException(pagoConTarjeta.Cantidad);

        if (!NumTarjetaValidator.ValidateTarjeta(pagoConTarjeta.NumeroTarjeta)) throw new InvalidCardNumberException(pagoConTarjeta.NumeroTarjeta);

        var client = await clientService.GetClientByUserIdAsync(user.Id);
        if (client is null) throw new ClientExceptions.ClientNotFoundException(user.Id);

        var clientCard = await creditCardService.GetCreditCardByCardNumber(pagoConTarjeta.NumeroTarjeta);
        if (clientCard is null) throw new PagoTarjetaCreditCardNotFoundException(pagoConTarjeta.NumeroTarjeta);

        var clientAccounts = await accountsService.GetCompleteAccountByClientIdAsync(client.Id);
        if (clientAccounts is null) throw new AccountNotFoundByClientIdException(client.Id);

        var cardAccount = clientAccounts.FirstOrDefault(a => a.TarjetaId == clientCard.Id);
        if (cardAccount is null) throw new CreditCardException.CreditCardNotAssignedException(pagoConTarjeta.NumeroTarjeta);

        var newBalance = cardAccount.Balance - pagoConTarjeta.Cantidad;
        if (newBalance < 0) throw new PagoTarjetaAccountInsufficientBalanceException(pagoConTarjeta.NumeroTarjeta);

        var updateAccountRequest = cardAccount.toUpdateAccountRequest();
        updateAccountRequest.Balance = newBalance;

        var updatedAccount = await accountsService.UpdateAccountAsync(cardAccount.Id, updateAccountRequest);
        logger.LogInformation($"New balance after Credit Card Payment: {updatedAccount.Balance}");

        var now = DateTime.UtcNow;
        Movimiento newMovimiento = new Movimiento
        {
            ClienteGuid = client.Id,
            PagoConTarjeta = pagoConTarjeta,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        var movimientoSaved = await movimientoRepository.AddMovimientoAsync(newMovimiento);

        await EnviarNotificacionCreacionAsync(user, movimientoSaved);
        return movimientoSaved;
    }

    /// <summary>
    /// Añade una transferencia entre cuentas.
    /// </summary>
    /// <param name="user">Usuario que realiza la acción (cliente del banco)</param>
    /// <param name="transferencia">Datos de la transferencia (IBANs, cantidad, beneficiario)</param>
    /// <returns>Movimiento de transferencia creado</returns>
    /// <exception cref="TransferSameIbanException">Lanzado cuando el IBAN de origen y destino son iguales</exception>
    /// <exception cref="TransferInvalidAmountException">Lanzado cuando la cantidad de la transferencia es menor o igual a cero</exception>
    /// <exception cref="InvalidSourceIbanException">Lanzado cuando el IBAN de origen no es válido</exception>
    /// <exception cref="InvalidDestinationIbanException">Lanzado cuando el IBAN de destino no es válido</exception>
    /// <exception cref="ClientNotFoundException">Lanzado cuando el cliente asociado al usuario no se encuentra</exception>
    /// <exception cref="AccountsExceptions.AccountNotFoundByIban">Lanzado cuando no se encuentra la cuenta origen</exception>
    /// <exception cref="AccountsExceptions.AccountUnknownIban">Lanzado cuando la cuenta de origen no pertenece al cliente</exception>
    /// <exception cref="TransferInsufficientBalance">Lanzado cuando la cuenta origen no tiene suficiente saldo</exception>
    /// <exception cref="AccountsExceptions.AccountNotFoundByIban">Lanzado cuando la cuenta de destino no se encuentra</exception>
    /// <exception cref="UserNotFoundException">Lanzado cuando el usuario asociado al cliente de destino no se encuentra</exception>
    /// <exception cref="ClientExceptions.ClientNotFoundException">Lanzado cuando no se encuentra el cliente asociado a la cuenta de destino</exception>
    /// <exception cref="Exception">Lanzado para otros errores inesperados</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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

    
    /// <summary>
    /// Revoca una transferencia previamente realizada.
    /// </summary>
    /// <param name="user">Usuario que solicita la revocación (cliente del banco)</param>
    /// <param name="movimientoTransferenciaGuid">Identificador único del movimiento de transferencia a revocar</param>
    /// <returns>Movimiento original de la transferencia revocada</returns>
    /// <exception cref="MovimientoNotFoundException">Lanzado cuando no se encuentra el movimiento de transferencia a revocar</exception>
    /// <exception cref="NotRevocableMovimientoException">Lanzado cuando han pasado más de 24 horas desde la creación del movimiento original</exception>
    /// <exception cref="MovementIsNotTransferException">Lanzado cuando el movimiento no es una transferencia</exception>
    /// <exception cref="ClientExceptions.ClientNotFoundException">Lanzado cuando el cliente asociado al usuario no se encuentra</exception>
    /// <exception cref="AccountsExceptions.AccountUnknownIban">Lanzado cuando el usuario no es el propietario de la cuenta de origen</exception>
    /// <exception cref="AccountsExceptions.AccountNotFoundByIban">Lanzado cuando no se encuentra la cuenta de origen o la cuenta de destino</exception>
    /// <exception cref="PagoTarjetaAccountInsufficientBalanceException">Lanzado cuando la cuenta no tiene saldo suficiente para revertir la transferencia</exception>
    /// <exception cref="UserNotFoundException">Lanzado cuando el usuario asociado al cliente de destino no se encuentra</exception>
    /// <exception cref="Exception">Lanzado para otros errores inesperados</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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
    
    /// <summary>
    /// Obtiene un movimiento de pago utilizando su identificador (ID). Utiliza caché para optimizar las consultas.
    /// </summary>
    /// <param name="id">Identificador único del movimiento a recuperar</param>
    /// <returns>El movimiento asociado al ID proporcionado, o null si no se encuentra</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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
    
    /// <summary>
    /// Obtiene un movimiento de pago utilizando su GUID único. Utiliza caché para optimizar las consultas.
    /// </summary>
    /// <param name="id">Identificador único (GUID) del movimiento a recuperar</param>
    /// <returns>El movimiento asociado al GUID proporcionado, o null si no se encuentra</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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
    
    /// <summary>
    /// Envia una notificación de creación de un movimiento a través del WebSocket para un usuario.
    /// </summary>
    /// <param name="user">Usuario al que se enviará la notificación</param>
    /// <param name="t">Objeto del tipo T que contiene los datos del movimiento creado</param>
    /// <returns>Una tarea asíncrona que representa la operación de notificación</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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

    /// <summary>
    /// Envia una notificación de eliminación de un movimiento a través del WebSocket para un usuario.
    /// </summary>
    /// <param name="user">Usuario al que se enviará la notificación</param>
    /// <param name="t">Objeto del tipo T que contiene los datos del movimiento eliminado</param>
    /// <returns>Una tarea asíncrona que representa la operación de notificación</returns>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <version>1.0.0</version>
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