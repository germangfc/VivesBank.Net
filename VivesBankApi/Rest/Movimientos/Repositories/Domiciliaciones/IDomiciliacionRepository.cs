using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones;

public interface IDomiciliacionRepository
{
    Task<List<Domiciliacion>> GetAllDomiciliacionesAsync();
    Task<Domiciliacion> GetDomiciliacionByIdAsync(String id);
    Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion);
    Task<Domiciliacion> UpdateDomiciliacionAsync(String id, Domiciliacion domiciliacion);
    Task<Domiciliacion> DeleteDomiciliacionAsync(String id);
}