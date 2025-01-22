namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidCifException(string cif) : MovimientoException($"CIF invalido: {cif}");