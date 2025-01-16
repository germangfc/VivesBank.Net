namespace VivesBankApi.Rest.Movimientos.Errors;

public class MovimientoNotFoundError(string guid)
    : MovimientoError($"No se encontro el movimiento con el ID/Guid {guid}");
