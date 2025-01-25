namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionInvalidCuantityException(string id, decimal amount)
    : MovimientoException($"Invalid Direct Debit amount ({amount}), id {id}");
