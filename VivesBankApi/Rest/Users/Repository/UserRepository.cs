using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public class UserRepository : GenericRepository<BancoDbContext,User>, IUserRepository
{
    public UserRepository(BancoDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Dni == username);
    }

    public async Task<PagedList<User>> GetAllUsersPagedAsync(
        int pageNumber,
        int pageSize,
        string role,
        bool? isDeleted,
        string direction)
    {
        _logger.LogInformation("Fetching all users");
        var query = _dbSet.AsQueryable();
        
        query = query.Where(a => a.Role.ToString().ToUpper().Contains(role.Trim().ToUpper()));
        
        if (isDeleted.HasValue)
        {
            query = query.Where(a => a.IsDeleted == isDeleted.Value);
        }
        
        query = direction.ToLower() switch
        {
            "desc" => query.OrderByDescending(e => e.Dni),
            _ => query.OrderBy(e => e.Dni)
        };
        
        query = query.Skip(pageNumber * pageSize).Take(pageSize);
        
        List<User> users =  await query.ToListAsync();
        return new PagedList<User>(users, await _dbSet.CountAsync(), pageNumber, pageSize);
    }
}