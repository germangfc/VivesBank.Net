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
            Dni = user.Dni,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt.ToLocalTime(),
            UpdatedAt = user.UpdatedAt.ToLocalTime(),
            IsDeleted = user.IsDeleted
        };
    }
    
    public static User UpdateUserFromInput(this UserUpdateRequest request, User existingUser)
    {
        User user = existingUser;
        
        if (request.Dni != null)
        {
            user.Dni = request.Dni;
        }

        if (request.Password != null)
        {
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if (request.Role != null)
        {
            if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
            {
                user.Role = userRole;
            }
            else
            {
                throw new InvalidRoleException(request.Role);
            }
        }
        
        user.UpdatedAt = DateTime.UtcNow;
        return user;
    }

    public static User ToUser(this LoginRequest request)
    {
        User newUser = new User();
        {
            newUser.Dni = request.Dni;
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            return newUser;
        }
    }

    public static User toUser(this CreateUserRequest request)
    {
        if (Enum.TryParse<Role>(request.Role.Trim(), true, out var userRole))
        {
            
            User newUser = new User();
            {
                newUser.Dni = request.Dni;
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                newUser.Role = userRole;
                return newUser;
            }
        }
        else
        {
            throw new InvalidRoleException(request.Role);
        }
    }
}