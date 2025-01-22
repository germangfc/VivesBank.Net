namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class IbanDestinoInvalidoException(string iban) : MovimientoException($"Iban de destino invalido: {iban}");
