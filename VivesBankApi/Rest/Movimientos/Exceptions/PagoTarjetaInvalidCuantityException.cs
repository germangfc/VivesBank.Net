namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaInvalidCuantityException(decimal amount)
    : MovimientoException($"Invalid Card payment amount ({amount})");