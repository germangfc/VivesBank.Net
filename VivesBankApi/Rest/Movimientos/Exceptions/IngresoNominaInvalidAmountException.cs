namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class IngresoNominaInvalidAmountException(decimal amount)
    : MovimientoException($"Invalid Payroll Income amount ({amount})");