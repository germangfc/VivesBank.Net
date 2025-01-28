namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class InvalidCifException(string cif) : MovimientoException($"Invalid CIF: {cif}");