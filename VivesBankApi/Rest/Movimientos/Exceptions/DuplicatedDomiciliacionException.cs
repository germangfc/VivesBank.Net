namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DuplicatedDomiciliacionException(string iban): MovimientoException($"Domiciliación al IBAN {iban} ya existente");