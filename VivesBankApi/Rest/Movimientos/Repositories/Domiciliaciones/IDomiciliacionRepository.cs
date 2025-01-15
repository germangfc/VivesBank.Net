using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;

public interface IDomiciliacionRepository
{
    Task<List<Domiciliacion>> GetAllDomiciliacionesAsync();
    Task<Domiciliacion> GetDomiciliacionByIdAsync(ObjectId id);
    Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion);
    Task<Domiciliacion> UpdateDomiciliacionAsync(ObjectId id, Domiciliacion domiciliacion);
    Task<Domiciliacion> DeleteDomiciliacionAsync(ObjectId id);
}