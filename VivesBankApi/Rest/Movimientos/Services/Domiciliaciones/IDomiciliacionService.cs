using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;

public interface IDomiciliacionService
{
    Task<List<Domiciliacion>> FindAllDomiciliacionesAsync();
    Task<Domiciliacion> FindDomiciliacionByIdAsync(ObjectId id);
    Task<String> AddDomiciliacionAsync(Domiciliacion domiciliacion);
    Task<String> UpdateDomiciliacionAsync(ObjectId id, Domiciliacion domiciliacion);
    Task<Domiciliacion> DeleteDomiciliacionAsync(ObjectId id);
}