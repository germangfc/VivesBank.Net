using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;

public interface IDomiciliacionService
{
    Task<List<Domiciliacion>> FindAllDomiciliacionesAsync();
    Task<Domiciliacion> FindDomiciliacionByIdAsync(String id);
    Task<String> AddDomiciliacionAsync(Domiciliacion domiciliacion);
    Task<String> UpdateDomiciliacionAsync(String id, Domiciliacion domiciliacion);
    Task<Domiciliacion> DeleteDomiciliacionAsync(String id);
    Task<List<Domiciliacion>> FindDomiciliacionesActivasByClienteGiudAsync(String clienteGuid);
}