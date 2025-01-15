
using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovimientoNotFoundException(string guid)
    : MovimientoException($"No se encontró el movimiento con el ID/Guid {guid}");