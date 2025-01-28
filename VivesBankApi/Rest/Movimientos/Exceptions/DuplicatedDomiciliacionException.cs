namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DuplicatedDomiciliacionException(string iban)
    : MovimientoException($"Direct Debit to account with IBAN {iban} already exists");