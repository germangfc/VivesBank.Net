using Microsoft.OpenApi.Extensions;
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
            switch (request.Role)
            {
                case "User":
                    user.Role = Role.User;
                    break;
                case "Admin":
                    user.Role = Role.Admin;
                    break;
                case "SuperAdmin":
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