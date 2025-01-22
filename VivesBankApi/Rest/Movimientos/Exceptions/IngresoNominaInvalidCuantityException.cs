namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class IngresoNominaInvalidCuantityException(decimal cantidad)
    : MovimientoException($"La cantidad de ingreso de nomina no puede ser negativa ({cantidad})");