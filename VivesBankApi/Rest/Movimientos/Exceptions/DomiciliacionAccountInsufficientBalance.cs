namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionAccountInsufficientBalance(string IBAN) 
    : MovimientoException($"Insufficient balance for direct debit from account IBAN {IBAN} ");