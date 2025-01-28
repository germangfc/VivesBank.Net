namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class NotRevocableMovimientoException(string movementGuid)
    :MovimientoException($"Not revocable movement with Id {movementGuid}");