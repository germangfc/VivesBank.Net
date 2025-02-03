namespace VivesBankApi.Rest.Movimientos.Errors;

/// <summary>
/// Constructor de la clase MovimientoNotFoundError.
/// </summary>
/// <param name="guid">El GUID del movimiento que no se pudo encontrar.</param>
public class MovimientoNotFoundError(string guid)
    : MovimientoError($"No se encontro el movimiento con el ID/Guid {guid}");
