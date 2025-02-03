using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Utils.GenericStorage.JSON;

namespace VivesBankApi.Rest.Users.Service;

/// <summary>
    /// Interfaz para el servicio de usuarios, que define las operaciones relacionadas con la gestión de usuarios.
    /// </summary>
    public interface IUserService : IGenericStorageJson<User>
    {
        /// <summary>
        /// Obtiene una lista de usuarios paginada, con opciones de filtrado por rol y estado de eliminación.
        /// </summary>
        /// <param name="pageNumber">Número de página para la paginación.</param>
        /// <param name="pageSize">Número de usuarios por página.</param>
        /// <param name="role">Rol de los usuarios a filtrar.</param>
        /// <param name="isDeleted">Estado de eliminación de los usuarios (true o false).</param>
        /// <param name="direction">Dirección del orden de los resultados (asc o desc).</param>
        /// <returns>Una lista paginada de respuestas de usuario.</returns>
        Task<PagedList<UserResponse>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string role,
            bool? isDeleted,
            string direction);

        /// <summary>
        /// Obtiene todos los usuarios sin paginación.
        /// </summary>
        /// <returns>Una lista de todos los usuarios.</returns>
        Task<List<User>> GetAll();

        /// <summary>
        /// Obtiene los detalles de un usuario por su ID.
        /// </summary>
        /// <param name="id">El ID del usuario a obtener.</param>
        /// <returns>Los detalles del usuario.</returns>
        Task<UserResponse> GetUserByIdAsync(String id);

        /// <summary>
        /// Agrega un nuevo usuario.
        /// </summary>
        /// <param name="userRequest">La información del usuario a agregar.</param>
        /// <returns>El usuario creado.</returns>
        Task<UserResponse> AddUserAsync(CreateUserRequest userRequest);

        /// <summary>
        /// Obtiene los detalles de un usuario por su nombre de usuario (DNI).
        /// </summary>
        /// <param name="username">El nombre de usuario (DNI) del usuario a obtener.</param>
        /// <returns>Los detalles del usuario.</returns>
        Task<UserResponse> GetUserByUsernameAsync(String username);

        /// <summary>
        /// Obtiene los detalles del usuario autenticado (el "usuario actual").
        /// </summary>
        /// <returns>Los detalles del usuario autenticado.</returns>
        Task<UserResponse> GettingMyUserData();

        /// <summary>
        /// Actualiza la información de un usuario existente.
        /// </summary>
        /// <param name="key">El identificador del usuario a actualizar.</param>
        /// <param name="request">Los nuevos detalles del usuario.</param>
        /// <returns>El usuario actualizado.</returns>
        Task<UserResponse> UpdateUserAsync(String key, UserUpdateRequest request);

        /// <summary>
        /// Elimina un usuario de forma lógica o física.
        /// </summary>
        /// <param name="id">El ID del usuario a eliminar.</param>
        /// <param name="logically">Indica si la eliminación debe ser lógica (no borrado físico) o no.</param>
        Task DeleteUserAsync(String id, bool logically);

        /// <summary>
        /// Realiza el inicio de sesión de un usuario con su DNI y contraseña.
        /// </summary>
        /// <param name="request">Los detalles del inicio de sesión.</param>
        /// <returns>El usuario autenticado si las credenciales son correctas, de lo contrario null.</returns>
        Task<User?> LoginUser(LoginRequest request);

        /// <summary>
        /// Actualiza la contraseña del usuario autenticado.
        /// </summary>
        /// <param name="request">Los detalles de la nueva contraseña.</param>
        /// <returns>El usuario actualizado con la nueva contraseña.</returns>
        Task<User?> UpdateMyPassword(UpdatePasswordRequest request);

        /// <summary>
        /// Elimina la cuenta del usuario autenticado.
        /// </summary>
        Task DeleteMeAsync();

        /// <summary>
        /// Registra un nuevo usuario utilizando la información proporcionada.
        /// </summary>
        /// <param name="request">Los detalles de la solicitud de registro.</param>
        /// <returns>El usuario registrado.</returns>
        Task<User?> RegisterUser(LoginRequest request);
    }