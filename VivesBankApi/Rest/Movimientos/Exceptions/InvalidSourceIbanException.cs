namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidSourceIbanException(string iban): MovimientoException($"Iban de origen invalido: {iban}");
