using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class DomiciliacionNotFoundException(ObjectId id) : MovimientoException($"Domiciliacion not found with ID {id}");
