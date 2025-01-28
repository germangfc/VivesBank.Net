namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionAccountInsufficientBalanceException(string IBAN) 
    : MovimientoException($"Insufficient balance for direct debit from account IBAN {IBAN} ");