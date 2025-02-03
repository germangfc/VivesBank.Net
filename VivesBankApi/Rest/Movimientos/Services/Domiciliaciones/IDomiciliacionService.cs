using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Services.Domiciliaciones;

public interface IDomiciliacionService
{
    /// <summary>
    /// Obtiene todas las domiciliaciones existentes.
    /// </summary>
    /// <remarks>
    /// Este método devuelve una lista con todas las domiciliaciones disponibles en el sistema.
    /// </remarks>
    /// <returns>Lista de domiciliaciones</returns>
    Task<List<Domiciliacion>> FindAllDomiciliacionesAsync();

    /// <summary>
    /// Obtiene una domiciliación por su ID.
    /// </summary>
    /// <param name="id">ID de la domiciliación</param>
    /// <remarks>
    /// Este método devuelve los detalles de una domiciliación específica identificada por su ID.
    /// Si no se encuentra la domiciliación, se lanzará una excepción.
    /// </remarks>
    /// <returns>Domiciliación correspondiente al ID</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    Task<Domiciliacion> FindDomiciliacionByIdAsync(String id);

    /// <summary>
    /// Agrega una nueva domiciliación al sistema.
    /// </summary>
    /// <param name="domiciliacion">El objeto domiciliación que se va a agregar</param>
    /// <remarks>
    /// Este método permite agregar una nueva domiciliación al sistema. Devuelve la URL de la nueva domiciliación agregada.
    /// </remarks>
    /// <returns>URL de la domiciliación recién agregada</returns>
    Task<String> AddDomiciliacionAsync(Domiciliacion domiciliacion);

    /// <summary>
    /// Actualiza una domiciliación existente en el sistema.
    /// </summary>
    /// <param name="id">ID de la domiciliación que se va a actualizar</param>
    /// <param name="domiciliacion">Objeto con la nueva información de la domiciliación</param>
    /// <remarks>
    /// Este método permite actualizar los detalles de una domiciliación existente. Si no se encuentra la domiciliación,
    /// se lanzará una excepción.
    /// </remarks>
    /// <returns>URL de la domiciliación actualizada</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    Task<String> UpdateDomiciliacionAsync(String id, Domiciliacion domiciliacion);

    /// <summary>
    /// Elimina una domiciliación del sistema por su ID.
    /// </summary>
    /// <param name="id">ID de la domiciliación que se va a eliminar</param>
    /// <remarks>
    /// Este método elimina una domiciliación existente identificada por su ID. Si la domiciliación no existe,
    /// se lanzará una excepción.
    /// </remarks>
    /// <returns>Detalles de la domiciliación eliminada</returns>
    /// <exception cref="DomiciliacionNotFoundException">Si no se encuentra la domiciliación con el ID proporcionado.</exception>
    Task<Domiciliacion> DeleteDomiciliacionAsync(String id);

    /// <summary>
    /// Obtiene las domiciliaciones activas de un cliente por su GUID.
    /// </summary>
    /// <param name="clienteGuid">GUID del cliente</param>
    /// <remarks>
    /// Este método devuelve todas las domiciliaciones activas asociadas al cliente identificado por su GUID.
    /// </remarks>
    /// <returns>Lista de domiciliaciones activas para el cliente</returns>
    Task<List<Domiciliacion>> FindDomiciliacionesActivasByClienteGiudAsync(String clienteGuid);
}
