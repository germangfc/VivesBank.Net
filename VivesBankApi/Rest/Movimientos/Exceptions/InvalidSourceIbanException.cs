namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidSourceIbanException(string iban)
    : MovimientoException($"Origin IBAN not valid: {iban}");
