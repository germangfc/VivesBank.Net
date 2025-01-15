using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Repository;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(String username);
}