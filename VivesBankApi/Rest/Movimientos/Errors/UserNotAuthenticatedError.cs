namespace VivesBankApi.Rest.Movimientos.Errors;

/// <summary>
/// Constructor de la clase UserNotAuthenticatedError.
/// </summary>
public class UserNotAuthenticatedError() : MovimientoError($"User is not authenticated");