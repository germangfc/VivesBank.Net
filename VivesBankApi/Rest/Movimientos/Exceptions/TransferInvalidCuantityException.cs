namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class TransferInvalidCuantityException(decimal amount)
    : MovimientoException($"Invalid Transfer amount ({amount})");
