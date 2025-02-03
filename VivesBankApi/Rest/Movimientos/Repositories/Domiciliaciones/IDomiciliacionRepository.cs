using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;
namespace VivesBankApi.Rest.Movimientos.Repositories.Domiciliaciones
{
    /// <summary>
    /// Interfaz para definir los métodos CRUD (crear, leer, actualizar, eliminar) y otros métodos de consulta
    /// relacionados con las domiciliaciones.
    /// </summary>
    /// <remarks>
    /// Esta interfaz se implementa en la clase <see cref="DomiciliacionRepository"/> y proporciona métodos para
    /// interactuar con la base de datos MongoDB para gestionar los datos de domiciliaciones.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public interface IDomiciliacionRepository
    {
        /// <summary>
        /// Obtiene todas las domiciliaciones registradas en la base de datos.
        /// </summary>
        /// <returns>Una lista de todas las domiciliaciones.</returns>
        Task<List<Domiciliacion>> GetAllDomiciliacionesAsync();

        /// <summary>
        /// Obtiene todas las domiciliaciones activas registradas en la base de datos.
        /// </summary>
        /// <returns>Una lista de domiciliaciones activas.</returns>
        Task<List<Domiciliacion>> GetAllDomiciliacionesActivasAsync();

        /// <summary>
        /// Obtiene una domiciliación específica por su id.
        /// </summary>
        /// <param name="id">El id de la domiciliación a obtener.</param>
        /// <returns>La domiciliación con el id especificado o null si no se encuentra.</returns>
        Task<Domiciliacion> GetDomiciliacionByIdAsync(string id);

        /// <summary>
        /// Agrega una nueva domiciliación a la base de datos.
        /// </summary>
        /// <param name="domiciliacion">El objeto <see cref="Domiciliacion"/> a agregar.</param>
        /// <returns>La domiciliación que fue agregada.</returns>
        Task<Domiciliacion> AddDomiciliacionAsync(Domiciliacion domiciliacion);

        /// <summary>
        /// Actualiza una domiciliación existente en la base de datos.
        /// </summary>
        /// <param name="id">El id de la domiciliación a actualizar.</param>
        /// <param name="domiciliacion">El objeto <see cref="Domiciliacion"/> con los nuevos datos.</param>
        /// <returns>La domiciliación actualizada.</returns>
        Task<Domiciliacion> UpdateDomiciliacionAsync(string id, Domiciliacion domiciliacion);

        /// <summary>
        /// Elimina una domiciliación de la base de datos.
        /// </summary>
        /// <param name="id">El id de la domiciliación a eliminar.</param>
        /// <returns>La domiciliación eliminada.</returns>
        Task<Domiciliacion> DeleteDomiciliacionAsync(string id);

        /// <summary>
        /// Obtiene todas las domiciliaciones activas asociadas a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar las domiciliaciones activas.</param>
        /// <returns>Una lista de domiciliaciones activas asociadas al cliente.</returns>
        Task<List<Domiciliacion>> GetDomiciliacionesActivasByClienteGiudAsync(string clienteGuid);

        /// <summary>
        /// Obtiene todas las domiciliaciones asociadas a un cliente específico.
        /// </summary>
        /// <param name="clientGuid">El GUID del cliente para filtrar las domiciliaciones.</param>
        /// <returns>Una lista de domiciliaciones asociadas al cliente.</returns>
        Task<List<Domiciliacion>> GetDomiciliacionByClientGuidAsync(string clientGuid);
    }
}
