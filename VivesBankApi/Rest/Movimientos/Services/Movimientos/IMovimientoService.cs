using MongoDB.Bson;
using VivesBankApi.Rest.Movimientos.Models;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Movimientos.Services.Movimientos;

public interface IMovimientoService : IGenericStorageJson<Movimiento>
{
    /// <summary>
    /// Obtiene todos los movimientos.
    /// </summary>
    /// <remarks>
    /// Este método devuelve una lista de todos los movimientos registrados en el sistema.
    /// </remarks>
    /// <returns>Lista de movimientos</returns>
    Task<List<Movimiento>> FindAllMovimientosAsync();

    /// <summary>
    /// Obtiene un movimiento por su ID.
    /// </summary>
    /// <param name="id">ID del movimiento</param>
    /// <remarks>
    /// Este método devuelve el movimiento asociado al ID proporcionado.
    /// </remarks>
    /// <returns>Movimiento encontrado</returns>
    Task<Movimiento> FindMovimientoByIdAsync(string id);

    /// <summary>
    /// Obtiene un movimiento por su GUID.
    /// </summary>
    /// <param name="guid">GUID del movimiento</param>
    /// <remarks>
    /// Este método devuelve el movimiento asociado al GUID proporcionado.
    /// </remarks>
    /// <returns>Movimiento encontrado</returns>
    Task<Movimiento> FindMovimientoByGuidAsync(string guid);

    /// <summary>
    /// Obtiene todos los movimientos asociados a un cliente, identificado por su ID.
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <remarks>
    /// Este método devuelve una lista de los movimientos asociados al cliente.
    /// </remarks>
    /// <returns>Lista de movimientos del cliente</returns>
    Task<List<Movimiento>> FindAllMovimientosByClientAsync(string clienteId);

    /// <summary>
    /// Agrega un nuevo movimiento.
    /// </summary>
    /// <param name="movimiento">Movimiento a agregar</param>
    /// <remarks>
    /// Este método agrega un nuevo movimiento a la base de datos.
    /// </remarks>
    /// <returns>Movimiento agregado</returns>
    Task<Movimiento> AddMovimientoAsync(Movimiento movimiento);

    /// <summary>
    /// Actualiza un movimiento existente.
    /// </summary>
    /// <param name="id">ID del movimiento a actualizar</param>
    /// <param name="movimiento">Datos del movimiento actualizados</param>
    /// <remarks>
    /// Este método actualiza los datos de un movimiento según el ID proporcionado.
    /// </remarks>
    /// <returns>Movimiento actualizado</returns>
    Task<Movimiento> UpdateMovimientoAsync(string id, Movimiento movimiento);

    /// <summary>
    /// Elimina un movimiento por su ID.
    /// </summary>
    /// <param name="id">ID del movimiento a eliminar</param>
    /// <remarks>
    /// Este método elimina el movimiento especificado por su ID.
    /// </remarks>
    /// <returns>Movimiento eliminado</returns>
    Task<Movimiento> DeleteMovimientoAsync(string id);

    /// <summary>
    /// Agrega una domiciliación a un usuario.
    /// </summary>
    /// <param name="user">Usuario asociado a la domiciliación</param>
    /// <param name="domiciliacion">Datos de la domiciliación a agregar</param>
    /// <remarks>
    /// Este método agrega una domiciliación al usuario.
    /// </remarks>
    /// <returns>Domiciliación agregada</returns>
    Task<Domiciliacion> AddDomiciliacionAsync(User user, Domiciliacion domiciliacion);

    /// <summary>
    /// Agrega un ingreso de nómina a un usuario.
    /// </summary>
    /// <param name="user">Usuario asociado al ingreso de nómina</param>
    /// <param name="ingresoDeNomina">Datos del ingreso de nómina a agregar</param>
    /// <remarks>
    /// Este método agrega un ingreso de nómina al usuario.
    /// </remarks>
    /// <returns>Ingreso de nómina agregado</returns>
    Task<Movimiento> AddIngresoDeNominaAsync(User user, IngresoDeNomina ingresoDeNomina);

    /// <summary>
    /// Agrega un pago con tarjeta a un usuario.
    /// </summary>
    /// <param name="user">Usuario asociado al pago con tarjeta</param>
    /// <param name="pagoConTarjeta">Datos del pago con tarjeta a agregar</param>
    /// <remarks>
    /// Este método agrega un pago con tarjeta al usuario.
    /// </remarks>
    /// <returns>Pago con tarjeta agregado</returns>
    Task<Movimiento> AddPagoConTarjetaAsync(User user, PagoConTarjeta pagoConTarjeta);

    /// <summary>
    /// Agrega una transferencia a un usuario.
    /// </summary>
    /// <param name="user">Usuario asociado a la transferencia</param>
    /// <param name="transferencia">Datos de la transferencia a agregar</param>
    /// <remarks>
    /// Este método agrega una transferencia al usuario.
    /// </remarks>
    /// <returns>Transferencia agregada</returns>
    Task<Movimiento> AddTransferenciaAsync(User user, Transferencia transferencia);

    /// <summary>
    /// Revoca una transferencia.
    /// </summary>
    /// <param name="user">Usuario que solicita la revocación</param>
    /// <param name="movimientoTransferenciaGuid">GUID de la transferencia a revocar</param>
    /// <remarks>
    /// Este método revoca una transferencia, según el GUID proporcionado.
    /// </remarks>
    /// <returns>Movimiento de transferencia revocada</returns>
    Task<Movimiento> RevocarTransferencia(User user, string movimientoTransferenciaGuid);
}
