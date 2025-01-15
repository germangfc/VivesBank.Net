using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Mapper;

class UserMapper
{
    protected UserMapper(){}
    
    public static UserResponse ToUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.GetType().Name,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsDeleted = user.IsDeleted
        };
    }
    
    public static UserResponse ToUserResponse(User response)
    {
        return new UserResponse
        {
            Id = response.Id,
            Username = response.Username,
            Role = response.Role.GetType().Name,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            IsDeleted = response.IsDeleted
        };
    }
    
    public static User UpdateUserFromInput(UserUpdateRequest request, User existingUser)
    {
        User user = existingUser;
        
        if (request.Username != null)
        {
            user.Username = request.Username;
        }

        if (request.Password != null)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (request.Role != null)
        {
            switch (request.Role.ToLower())
            {
                case "user":
                    user.Role = Role.User;
                    break;
                case "admin":
                    user.Role = Role.Admin;
                    break;
                case "auperadmin":
                    user.Role = Role.SuperAdmin;
                    break;
                default:
                    throw new InvalidUserException(request.Role);
            } 
        }
        
        user.UpdatedAt = DateTime.Now;
        return user;
    }
}