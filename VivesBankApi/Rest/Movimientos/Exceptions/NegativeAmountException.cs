namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class NegativeAmountException(decimal amount) : MovimientoException($"Negative amount {amount}");
