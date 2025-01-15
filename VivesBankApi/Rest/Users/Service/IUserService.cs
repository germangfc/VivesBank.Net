using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Service;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetUserByIdAsync(String id);
    Task<User> AddUserAsync(User user);
    Task<User?> GetUserByUsernameAsync(String username);
    Task<User> UpdateUserAsync(String key, UserUpdateRequest request);
    Task DeleteUserAsync(String id, bool logically);
}