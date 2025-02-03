using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository
{
    /// <summary>
    /// Interfaz para operaciones relacionadas con el repositorio de usuarios.
    /// Hereda de <see cref="IGenericRepository{User}"/> para operaciones CRUD básicas.
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        /// <summary>
        /// Obtiene un usuario por su nombre de usuario.
        /// </summary>
        /// <param name="username">El nombre de usuario del usuario a obtener.</param>
        /// <returns>El usuario encontrado o null si no se encuentra.</returns>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// Obtiene una lista de usuarios paginada, con la opción de filtrar por rol y estado de eliminación.
        /// </summary>
        /// <param name="pageNumber">Número de página para la paginación (comienza en 0).</param>
        /// <param name="pageSize">Cantidad de usuarios por página.</param>
        /// <param name="role">Rol de los usuarios a obtener (puede ser vacío para no filtrar por rol).</param>
        /// <param name="isDeleted">Indica si se deben filtrar usuarios eliminados o no.</param>
        /// <param name="direction">Dirección del ordenamiento, puede ser "asc" o "desc".</param>
        /// <returns>Una lista paginada de usuarios.</returns>
        Task<PagedList<User>> GetAllUsersPagedAsync(
            int pageNumber,
            int pageSize,
            string role,
            bool? isDeleted,
            string direction);
    }
}