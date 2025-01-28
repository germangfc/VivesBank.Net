namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class TransferInsufficientBalance(string iban)
    : MovimientoException($"Insufficient balance for transfer in account with IBAN {iban}");
