namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaAccountInsufficientBalance(string cardNumber) 
    : MovimientoException($"Insufficient balance for card payment from card {cardNumber} ");
