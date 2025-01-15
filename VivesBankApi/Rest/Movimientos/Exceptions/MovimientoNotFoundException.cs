
using MongoDB.Bson;

namespace VivesBankApi.Rest.Movimientos.Exceptions;

public class MovimientoNotFoundException : MovimientoException
{
    // Constructor with ObjectId
    public MovimientoNotFoundException(ObjectId id)
        : base($"No se encontró el movimiento con el ID {id}")
    {
    }

    // Overloaded constructor with string ID
    public MovimientoNotFoundException(string guid)
        : base($"No se encontró el movimiento con el GUID {guid}")
    {
    }
}