namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidCardNumberException(string cardNumber) : MovimientoException($"Invalid card number: {cardNumber}");