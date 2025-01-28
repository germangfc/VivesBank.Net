
using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovimientoNotFoundException(string guid)
    : MovimientoException($"Movement not found with ID/Guid {guid}");