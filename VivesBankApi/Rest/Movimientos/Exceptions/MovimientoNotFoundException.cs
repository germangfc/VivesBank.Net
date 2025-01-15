using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovimientoNotFoundException(ObjectId id) : MovimientoException($"No se encontró el movimiento con el ID {id}");