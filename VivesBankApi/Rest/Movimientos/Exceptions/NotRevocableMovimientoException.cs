namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovimientoNotRevocableException(string movementGuid)
    :MovimientoException($"Not revocable movement with Id {movementGuid}");