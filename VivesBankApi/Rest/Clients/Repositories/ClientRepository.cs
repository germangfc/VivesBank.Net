using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using VivesBankApi.Database;
using VivesBankApi.Rest.Clients.Models;

namespace VivesBankApi.Rest.Clients.Repositories;

public class ClientRepository : GenericRepository<BancoDbContext, Client>, IClientRepository
{
    public ClientRepository(BancoDbContext context, ILogger<ClientRepository> logger) : base(context, logger)
    {
    }
    
    public async Task<PagedList<Client>> GetAllClientsPagedAsync(
        int pageNumber,
        int pageSize,
        string name,
        bool? isDeleted,
        string direction)
    {
        _logger.LogInformation("Fetching all clients");
        var query = _dbSet.AsQueryable();
        
        query = query.Where(a => a.FullName.ToUpper().Contains(name.Trim().ToUpper()));
        
        if (isDeleted.HasValue)
        {
            query = query.Where(a => a.IsDeleted == isDeleted.Value);
        }
        
        query = direction.ToLower() switch
        {
            "desc" => query.OrderByDescending(e => e.FullName),
            _ => query.OrderBy(e => e.FullName)
        };
        
        query = query.Skip(pageNumber * pageSize).Take(pageSize);
        
        List<Client> clients =  await EntityFrameworkQueryableExtensions.ToListAsync(query);
        return new PagedList<Client>(clients, await EntityFrameworkQueryableExtensions.CountAsync(_dbSet), pageNumber, pageSize);
    }

    public async Task<Client?> getByUserIdAsync(string userId)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(
                _context.Set<Client>().Where(client => client.UserId == userId && !client.IsDeleted)
            );
    }
}