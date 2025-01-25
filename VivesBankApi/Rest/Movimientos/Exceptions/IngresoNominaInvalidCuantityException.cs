namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class IngresoNominaInvalidCuantityException(decimal amount)
    : MovimientoException($"Invalid Payroll Income amount ({amount})");