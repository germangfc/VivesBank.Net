using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using VivesBankApi.Database;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Repositories
{
    /// <summary>
    /// Repositorio que maneja las operaciones CRUD relacionadas con los clientes.
    /// Hereda de <see cref="GenericRepository{BancoDbContext, Client}"/> y proporciona métodos específicos 
    /// para interactuar con la base de datos de clientes.
    /// </summary>
    public class ClientRepository : GenericRepository<BancoDbContext, Client>, IClientRepository
    {
        /// <summary>
        /// Constructor del repositorio de clientes.
        /// </summary>
        /// <param name="context">El contexto de la base de datos que se utiliza para interactuar con la base de datos.</param>
        /// <param name="logger">El logger para registrar información y errores.</param>
        public ClientRepository(BancoDbContext context, ILogger<ClientRepository> logger) : base(context, logger)
        {
        }
        
        /// <summary>
        /// Obtiene una lista paginada de clientes, filtrada y ordenada según los parámetros proporcionados.
        /// </summary>
        /// <param name="pageNumber">El número de la página de resultados que se desea obtener (empezando desde 0).</param>
        /// <param name="pageSize">El tamaño de cada página de resultados.</param>
        /// <param name="name">Nombre del cliente para filtrar los resultados por nombre (se hace una búsqueda parcial).</param>
        /// <param name="isDeleted">Filtro opcional que indica si se deben obtener clientes eliminados o no.</param>
        /// <param name="direction">Dirección de ordenación: "asc" para ascendente, "desc" para descendente.</param>
        /// <returns>Una lista de clientes paginada que cumple con los criterios de filtrado y ordenación proporcionados.</returns>
        public async Task<PagedList<Client>> GetAllClientsPagedAsync(
            int pageNumber,
            int pageSize,
            string name,
            bool? isDeleted,
            string direction)
        {
            _logger.LogInformation("Fetching all clients");
            var query = _dbSet.AsQueryable();
            
            // Filtrar por nombre
            query = query.Where(a => a.FullName.ToUpper().Contains(name.Trim().ToUpper()));
            
            // Filtrar por si está eliminado
            if (isDeleted.HasValue)
            {
                query = query.Where(a => a.IsDeleted == isDeleted.Value);
            }
            
            // Ordenar por nombre
            query = direction.ToLower() switch
            {
                "desc" => query.OrderByDescending(e => e.FullName),
                _ => query.OrderBy(e => e.FullName)
            };
            
            // Paginación
            query = query.Skip(pageNumber * pageSize).Take(pageSize);
            
            // Obtener los resultados
            List<Client> clients = await EntityFrameworkQueryableExtensions.ToListAsync(query);
            
            // Retornar paginación
            return new PagedList<Client>(clients, await EntityFrameworkQueryableExtensions.CountAsync(_dbSet), pageNumber, pageSize);
        }

        /// <summary>
        /// Obtiene un cliente basado en su ID de usuario, asegurándose de que el cliente no haya sido marcado como eliminado.
        /// </summary>
        /// <param name="userId">El ID de usuario del cliente a buscar.</param>
        /// <returns>El cliente correspondiente si se encuentra, de lo contrario, null.</returns>
        public async Task<Client?> getByUserIdAsync(string userId)
        {
            return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(
                    _context.Set<Client>().Where(client => client.UserId == userId && !client.IsDeleted)
                );
        }
    }
}
