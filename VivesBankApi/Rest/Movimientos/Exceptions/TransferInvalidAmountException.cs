namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class TransferInvalidAmountException(decimal amount)
    : MovimientoException($"Invalid Transfer amount ({amount})");
