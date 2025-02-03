using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

/// <summary>
    /// Implementación del repositorio para la entidad <see cref="User"/>.
    /// Hereda de <see cref="GenericRepository{BancoDbContext, User}"/> y proporciona operaciones
    /// adicionales específicas para los usuarios.
    /// </summary>
    public class UserRepository : GenericRepository<BancoDbContext, User>, IUserRepository
    {
        private readonly ILogger<UserRepository> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="UserRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de la aplicación.</param>
        /// <param name="logger">El logger utilizado para registrar información de la actividad.</param>
        public UserRepository(BancoDbContext context, ILogger<UserRepository> logger) : base(context, logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un usuario por su DNI (que actúa como nombre de usuario).
        /// </summary>
        /// <param name="username">El DNI (nombre de usuario) del usuario a obtener.</param>
        /// <returns>El usuario encontrado o null si no existe.</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Dni == username);
        }

        /// <summary>
        /// Obtiene una lista de usuarios paginada, con la opción de filtrar por rol y estado de eliminación.
        /// </summary>
        /// <param name="pageNumber">Número de página para la paginación (comienza en 0).</param>
        /// <param name="pageSize">Cantidad de usuarios por página.</param>
        /// <param name="role">Rol de los usuarios a obtener (puede ser vacío para no filtrar por rol).</param>
        /// <param name="isDeleted">Indica si se deben filtrar usuarios eliminados o no.</param>
        /// <param name="direction">Dirección del ordenamiento, puede ser "asc" o "desc".</param>
        /// <returns>Una lista paginada de usuarios.</returns>
        public async Task<PagedList<User>> GetAllUsersPagedAsync(
            int pageNumber,
            int pageSize,
            string role,
            bool? isDeleted,
            string direction)
        {
            _logger.LogInformation("Fetching all users");
            var query = _dbSet.AsQueryable();

            // Filtrar por rol si se proporciona
            query = query.Where(a => a.Role.ToString().ToUpper().Contains(role.Trim().ToUpper()));
            
            // Filtrar por estado de eliminación si se proporciona
            if (isDeleted.HasValue)
            {
                query = query.Where(a => a.IsDeleted == isDeleted.Value);
            }

            // Ordenar según la dirección proporcionada
            query = direction.ToLower() switch
            {
                "desc" => query.OrderByDescending(e => e.Dni),
                _ => query.OrderBy(e => e.Dni)
            };

            // Paginación: omitir elementos previos y tomar el número de elementos especificados
            query = query.Skip(pageNumber * pageSize).Take(pageSize);

            // Ejecutar la consulta y devolver el resultado paginado
            List<User> users = await query.ToListAsync();
            return new PagedList<User>(users, await _dbSet.CountAsync(), pageNumber, pageSize);
        }
    }