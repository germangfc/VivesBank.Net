using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Users.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public class UserRepository : GenericRepository<UserDbContext, User>
{
    public UserRepository(UserDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
    }
}