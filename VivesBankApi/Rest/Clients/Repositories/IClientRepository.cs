using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Repositories
{
    /// <summary>
    /// Define los métodos necesarios para interactuar con los datos de los clientes.
    /// Esta interfaz hereda de IGenericRepository<Client> para operaciones comunes.
    /// </summary>
    public interface IClientRepository : IGenericRepository<Client>
    {
        /// <summary>
        /// Obtiene una lista de clientes paginada con opciones de filtro.
        /// </summary>
        /// <param name="pageNumber">Número de la página que se desea obtener.</param>
        /// <param name="pageSize">Número de clientes por página.</param>
        /// <param name="name">Filtro opcional por el nombre del cliente.</param>
        /// <param name="isDeleted">Filtro opcional para obtener clientes eliminados o no eliminados.</param>
        /// <param name="direction">Dirección de ordenación, por ejemplo, "asc" o "desc".</param>
        /// <returns>Una tarea que representa la operación asíncrona, con una lista paginada de clientes.</returns>
        public Task<PagedList<Client>> GetAllClientsPagedAsync(
            int pageNumber,
            int pageSize,
            string name,
            bool? isDeleted,
            string direction);

        /// <summary>
        /// Obtiene un cliente por su identificador de usuario.
        /// </summary>
        /// <param name="userId">El identificador de usuario del cliente.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el cliente encontrado o null si no existe.</returns>
        public Task<Client?> getByUserIdAsync(string userId);
    }
}
