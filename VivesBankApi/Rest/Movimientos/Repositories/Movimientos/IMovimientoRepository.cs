using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;

namespace VivesBankApi.Rest.Movimientos.Repositories.Movimientos
{
    /// <summary>
    /// Interfaz que define los métodos CRUD (crear, leer, actualizar, eliminar) y otros métodos de consulta
    /// relacionados con los movimientos bancarios.
    /// </summary>
    /// <remarks>
    /// Esta interfaz se implementa en la clase <see cref="MovimientoRepository"/> y proporciona métodos para
    /// interactuar con la base de datos MongoDB para gestionar los movimientos bancarios.
    /// </remarks>
    /// <author>VivesBank Team</author>
    public interface IMovimientoRepository
    {
        /// <summary>
        /// Obtiene todos los movimientos registrados en la base de datos.
        /// </summary>
        /// <returns>Una lista de todos los movimientos.</returns>
        Task<List<Movimiento>> GetAllMovimientosAsync();

        /// <summary>
        /// Obtiene un movimiento específico por su id.
        /// </summary>
        /// <param name="id">El id del movimiento a obtener.</param>
        /// <returns>El movimiento con el id especificado o null si no se encuentra.</returns>
        Task<Movimiento?> GetMovimientoByIdAsync(string id);

        /// <summary>
        /// Obtiene un movimiento específico por su GUID.
        /// </summary>
        /// <param name="guid">El GUID del movimiento a obtener.</param>
        /// <returns>El movimiento con el GUID especificado o null si no se encuentra.</returns>
        Task<Movimiento?> GetMovimientoByGuidAsync(string guid);

        /// <summary>
        /// Obtiene todos los movimientos de un cliente específico por su id.
        /// </summary>
        /// <param name="clienteId">El id del cliente para filtrar los movimientos.</param>
        /// <returns>Una lista de movimientos asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosByClientAsync(string clienteId);

        /// <summary>
        /// Agrega un nuevo movimiento a la base de datos.
        /// </summary>
        /// <param name="movimiento">El objeto <see cref="Movimiento"/> a agregar.</param>
        /// <returns>El movimiento que fue agregado.</returns>
        Task<Movimiento> AddMovimientoAsync(Movimiento movimiento);

        /// <summary>
        /// Actualiza un movimiento existente en la base de datos.
        /// </summary>
        /// <param name="id">El id del movimiento a actualizar.</param>
        /// <param name="movimiento">El objeto <see cref="Movimiento"/> con los nuevos datos.</param>
        /// <returns>El movimiento actualizado.</returns>
        Task<Movimiento> UpdateMovimientoAsync(string id, Movimiento movimiento);

        /// <summary>
        /// Elimina un movimiento de la base de datos.
        /// </summary>
        /// <param name="id">El id del movimiento a eliminar.</param>
        /// <returns>El movimiento eliminado.</returns>
        Task<Movimiento> DeleteMovimientoAsync(string id);

        /// <summary>
        /// Obtiene todos los movimientos de domiciliación asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de domiciliación.</param>
        /// <returns>Una lista de movimientos de domiciliación asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosDomiciliacionByClienteGuidAsync(string clienteGuid);

        /// <summary>
        /// Obtiene todos los movimientos de transferencia asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de transferencia.</param>
        /// <returns>Una lista de movimientos de transferencia asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosTransferenciaByClienteGuidAsync(string clienteGuid);

        /// <summary>
        /// Obtiene todos los movimientos de pago con tarjeta asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de pago con tarjeta.</param>
        /// <returns>Una lista de movimientos de pago con tarjeta asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosPagoConTarjetaByClienteGuidAsync(string clienteGuid);

        /// <summary>
        /// Obtiene todos los movimientos de recibo de nómina asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de recibo de nómina.</param>
        /// <returns>Una lista de movimientos de recibo de nómina asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosReciboDeNominaByClienteGuidAsync(string clienteGuid);

        /// <summary>
        /// Obtiene todos los movimientos de transferencia revocada asociados a un cliente específico.
        /// </summary>
        /// <param name="clienteGuid">El GUID del cliente para filtrar los movimientos de transferencia revocada.</param>
        /// <returns>Una lista de movimientos de transferencia revocada asociados al cliente.</returns>
        Task<List<Movimiento>> GetMovimientosTransferenciaRevocadaByClienteGuidAsync(string clienteGuid);
    }
}
