using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Service;

public interface IUserService
{
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> GetUserByIdAsync(String id);
    Task<UserResponse> AddUserAsync(CreateUserRequest userRequest);
    Task<UserResponse> GetUserByUsernameAsync(String username);
    Task<UserResponse> UpdateUserAsync(String key, UserUpdateRequest request);
    Task DeleteUserAsync(String id, bool logically);
}