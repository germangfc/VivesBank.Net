namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaInvalidAmountException(decimal amount)
    : MovimientoException($"Invalid Card payment amount ({amount})");