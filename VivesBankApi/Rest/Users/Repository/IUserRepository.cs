using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public class IUserRepository : GenericRepository<BancoDbContext, User>
{
    public IUserRepository(BancoDbContext context, ILogger<IUserRepository> logger) : base(context, logger)
    {
    }
}