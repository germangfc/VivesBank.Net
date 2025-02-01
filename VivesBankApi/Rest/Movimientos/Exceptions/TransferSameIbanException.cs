namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class TransferSameIbanException(string iban): 
    MovimientoException($"The origin and destination accounts must be different: {iban}");