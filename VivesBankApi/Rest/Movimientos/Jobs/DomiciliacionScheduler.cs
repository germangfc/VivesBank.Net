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

/// <summary>
    /// Clase que gestiona la ejecución de domiciliaciones programadas.
    /// </summary>
    /// <remarks>
    /// Esta clase procesa las domiciliaciones activas y las ejecuta de acuerdo con su periodicidad.
    /// Utiliza Quartz para la programación de tareas y WebSockets para enviar notificaciones.
    /// </remarks>
    /// <author>VivesBank Team</author>
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

        /// <summary>
        /// Constructor de la clase DomiciliacionScheduler.
        /// </summary>
        /// <param name="domiciliacionRepository">Repositorio para manejar domiciliaciones.</param>
        /// <param name="movimientoRepository">Repositorio para manejar movimientos bancarios.</param>
        /// <param name="accountsService">Servicio de cuentas bancarias.</param>
        /// <param name="userService">Servicio de usuarios.</param>
        /// <param name="clientService">Servicio de clientes.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        /// <param name="websocketHandler">Manejador de WebSocket para enviar notificaciones.</param>
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

        /// <summary>
        /// Método que se ejecuta de forma programada para procesar domiciliaciones.
        /// </summary>
        /// <param name="context">Contexto de ejecución del job de Quartz.</param>
        /// <remarks>
        /// Este método filtra las domiciliaciones activas que requieren ejecución
        /// y realiza las acciones necesarias (verificar saldo, actualizar cuentas, registrar movimientos).
        /// </remarks>
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
                        _logger.LogWarning($"Insufficient balance for Client: {domiciliacion.ClienteGuid}, Account: {domiciliacion.IbanOrigen}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error processing direct debit: {domiciliacion.Guid}, Account: {domiciliacion.IbanOrigen}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Ejecuta una domiciliación.
        /// </summary>
        /// <param name="domiciliacion">La domiciliación que se va a ejecutar.</param>
        /// <param name="originAccount">La cuenta de origen donde se descontará el monto.</param>
        /// <param name="date">Fecha de ejecución de la domiciliación.</param>
        /// <remarks>
        /// Este método verifica si el saldo es suficiente, actualiza el saldo de la cuenta
        /// y registra un nuevo movimiento bancario.
        /// </remarks>
        private async Task EjecutarDomiciliacionAsync(Domiciliacion domiciliacion, AccountCompleteResponse originAccount, DateTime date)
        {
            _logger.LogInformation($"Executing direct debit Client: {domiciliacion.ClienteGuid}, Company: {domiciliacion.NombreAcreedor}, Quantity: {domiciliacion.Cantidad}");

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

        /// <summary>
        /// Verifica si una domiciliación requiere ejecución según su periodicidad.
        /// </summary>
        /// <param name="domiciliacion">La domiciliación que se va a verificar.</param>
        /// <param name="ahora">La fecha y hora actual.</param>
        /// <returns>Retorna verdadero si la domiciliación necesita ejecución.</returns>
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

        /// <summary>
        /// Envía una notificación de ejecución a través de WebSocket.
        /// </summary>
        /// <typeparam name="T">Tipo de dato que se enviará en la notificación.</typeparam>
        /// <param name="userResponse">Respuesta del usuario destinatario de la notificación.</param>
        /// <param name="t">Datos que se incluyen en la notificación.</param>
        /// <remarks>
        /// Este método crea una notificación del tipo "Execute" y la envía al usuario
        /// a través de WebSocket para mantenerlo informado de la ejecución de la domiciliación.
        /// </remarks>
        public async Task EnviarNotificacionExecuteAsync<T>(UserResponse userResponse, T t)
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