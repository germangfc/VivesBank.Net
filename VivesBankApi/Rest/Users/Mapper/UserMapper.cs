using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Mapper;

public static class UserMapper
{
    public static UserResponse ToUserResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt.ToLocalTime(),
            UpdatedAt = user.UpdatedAt.ToLocalTime(),
            IsDeleted = user.IsDeleted
        };
    }
    
    public static User UpdateUserFromInput(this UserUpdateRequest request, User oldUser)
    {
        User user = oldUser;
        
        user.Username = request.Username;
    
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
        {
            user.Role = userRole;
        }
        else
        {
            throw new InvalidRoleException(request.Role);
        }
    
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }

    public static User ToUser(this CreateUserRequest request)
    {
        User newUser = new User();
        if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
        {
            newUser.Username = request.Username;
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            newUser.Role = userRole;
            return newUser;
        }
        throw new InvalidRoleException(request.Role); 
        
    }
}