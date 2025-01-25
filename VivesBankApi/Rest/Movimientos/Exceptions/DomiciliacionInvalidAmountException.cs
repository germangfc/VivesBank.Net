namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionInvalidAmountException(string id, decimal amount)
    : MovimientoException($"Invalid Direct Debit amount ({amount}), id {id}");
