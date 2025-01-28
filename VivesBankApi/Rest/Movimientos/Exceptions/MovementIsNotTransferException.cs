namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovementIsNotTransferException(string guid)
    : MovimientoException($"Movement with Id {guid} is not a transfer");