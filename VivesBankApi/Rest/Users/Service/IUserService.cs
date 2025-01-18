using VivesBankApi.Rest.Users.Dtos;

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
    Task<UserResponse> UpdateUserAsync(String key, UserUpdateRequest request);
    Task DeleteUserAsync(String id, bool logically);
}