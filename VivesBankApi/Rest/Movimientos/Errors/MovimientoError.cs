namespace VivesBankApi.Rest.Movimientos.Errors;

/// <summary>
/// Constructor de la clase MovimientoError.
/// </summary>
/// <param name="message">Mensaje de error que describe el problema.</param>
public class MovimientoError(string message) : Error(message);
