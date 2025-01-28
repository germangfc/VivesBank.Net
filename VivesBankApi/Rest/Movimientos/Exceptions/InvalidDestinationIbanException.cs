namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidDestinationIbanException(string iban) 
    : MovimientoException($"Destination IBAN not valid: {iban}");
