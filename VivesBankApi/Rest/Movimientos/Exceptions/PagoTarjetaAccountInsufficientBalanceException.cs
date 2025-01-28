namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaAccountInsufficientBalanceException(string cardNumber) 
    : MovimientoException($"Insufficient balance for card payment from card {cardNumber} ");
