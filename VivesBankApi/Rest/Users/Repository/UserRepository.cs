using ApiFunkosCS.Utils.GenericRepository;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public class UserRepository : GenericRepository<BancoDbContext, User>, IUserRepository
{
    public UserRepository(BancoDbContext context, ILogger<UserRepository> logger) : base(context, logger)
    {
    }
    
}