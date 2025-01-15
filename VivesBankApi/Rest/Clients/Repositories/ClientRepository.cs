using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Database;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Repositories;

public class ClientRepository : GenericRepository<BancoDbContext, Client>, IClientRepository
{
    public ClientRepository(BancoDbContext context, ILogger<ClientRepository> logger) : base(context, logger)
    {
    } 
}