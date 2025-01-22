using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Repositories;

public interface IClientRepository : IGenericRepository<Client>
{
    public Task<PagedList<Client>> GetAllClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string name,
        bool? isDeleted,
        string direction);
    
    public Task<Client?> getByUserIdAsync(string userId);
}