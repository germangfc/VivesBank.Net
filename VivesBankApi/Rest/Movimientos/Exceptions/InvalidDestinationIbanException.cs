namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidDestinationIbanException(string iban) : MovimientoException($"Iban de destino invalido: {iban}");
