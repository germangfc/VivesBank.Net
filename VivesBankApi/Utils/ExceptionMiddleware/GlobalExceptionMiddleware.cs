using System.Net;
using System.Text.Json;
using VivesBankApi.Rest.Clients.Exceptions;
using VivesBankApi.Rest.Movimientos.Exceptions;
using VivesBankApi.Rest.Products.BankAccounts.Exceptions;
using VivesBankApi.Rest.Users.Exceptions;

namespace ApiFunkosCS.Utils.ExceptionMiddleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Definir el código de estado HTTP por defecto
            var statusCode = HttpStatusCode.BadRequest;
            var errorResponse = new { message = "An unexpected error occurred." };

            // Manejar tipos de excepciones personalizadas
            switch (exception)
            {
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, "Invalid operation.");
                    break;
                
                /**************** NOTFOUND EXCEPTIONS *****************************************/
                case MovimientoNotFoundException:
                case DomiciliacionNotFoundException:
                case UserNotFoundException:
                case AccountsExceptions.AccountNotFoundException:
                case AccountsExceptions.AccountNotFoundByIban:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                        
                /**************** MOVIMIENTOS EXCEPTIONS *****************************************/
                
                case DomiciliacionInvalidCuantityException:
                case IngresoNominaInvalidCuantityException:
                case PagoTarjetaInvalidCuantityException:
                case InvalidSourceIbanException:
                case InvalidCardNumberException:
                case InvalidDestinationIbanException:
                case InvalidCifException:
                case DuplicatedDomiciliacionException:
                case NegativeAmountException:
                case AccountNotFoundByClientId:
                case PagoTarjetaAccountInsufficientBalance:
                case DomiciliacionAccountInsufficientBalance:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;  
                
                /**************** USER EXCEPTIONS *****************************************/
                
                case InvalidDniException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                case InvalidRoleException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                case UserAlreadyExistsException:
                    statusCode = HttpStatusCode.Conflict;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                /************************** ACCOUNT EXCEPTIONS *****************************************************/
                case AccountsExceptions.AccountNotCreatedException:
                case AccountsExceptions.AccountUnknownIban:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                case AccountsExceptions.AccountIbanNotGeneratedException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                /************************** CLIENT EXCEPTIONS *****************************************************/
                case ClientExceptions.ClientAlreadyExistsException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                
                case ClientExceptions.ClientNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorResponse = new { message = exception.Message };
                    logger.LogWarning(exception, exception.Message);
                    break;
                
                default:
                    logger.LogError(exception, "An unhandled exception occurred.");
                    break;
                
                /************************** STORAGE EXCEPTIONS *****************************************************/
                

            }

            // Configurar la respuesta HTTP
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse);
            return context.Response.WriteAsync(jsonResponse);
        }
    }