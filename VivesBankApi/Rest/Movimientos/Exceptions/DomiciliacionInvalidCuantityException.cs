namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionInvalidCuantityException(string id, decimal cantidad)
    : MovimientoException($"La cantidad de la domiciliación {id} no puede ser negativa ({cantidad})");
