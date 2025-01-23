namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaInvalidCuantityException(decimal cantidad)
    : MovimientoException($"La cantidad de pago con tarjeta no puede ser negativa ({cantidad})");