namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class IbanOrigenInvalidoException(string iban): MovimientoException($"Iban de origen invalido: {iban}");
