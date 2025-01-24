using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Service;

public interface IUserService
{
    Task<PagedList<UserResponse>> GetAllUsersAsync(
        int pageNumber, 
        int pageSize,
        string role,
        bool? isDeleted,
        string direction);
    Task<UserResponse> GetUserByIdAsync(String id);
    Task<UserResponse> AddUserAsync(CreateUserRequest userRequest);
    Task<UserResponse> GetUserByUsernameAsync(String username);
    Task<UserResponse> GettingMyUserData();
    Task<UserResponse> UpdateUserAsync(String key, UserUpdateRequest request);
    Task DeleteUserAsync(String id, bool logically);
    String GenerateJwtToken(User user);
    Task<User?> LoginUser(LoginRequest request);
    Task<User?> UpdateMyPassword(UpdatePasswordRequest request);
    Task<User?> DeleteMeAsync();
    Task<User?> RegisterUser(LoginRequest request);
}