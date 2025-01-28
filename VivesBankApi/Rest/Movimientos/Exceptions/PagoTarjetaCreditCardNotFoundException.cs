namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class PagoTarjetaCreditCardNotFoundException(string cardNumber)
    : MovimientoException($"Credit card payment: card with card number {cardNumber} not found ");
