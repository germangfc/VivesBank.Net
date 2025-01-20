using ApiFunkosCS.Utils.GenericRepository;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByUsernameAsync(String username);

    Task<PagedList<User>> GetAllUsersPagedAsync(
        int pageNumber,
        int pageSize,
        string role,
        bool? isDeleted,
        string direction);
}