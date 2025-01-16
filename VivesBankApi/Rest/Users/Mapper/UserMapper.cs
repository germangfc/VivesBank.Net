using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;

namespace VivesBankApi.Rest.Users.Mapper;

public class UserMapper
{
    protected  UserMapper(){}
    
    public static UserResponse ToUserResponse(User user)
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
                case "superadmin":
                    user.Role = Role.SuperAdmin;
                    break;
                default:
                    throw new InvalidUserException($"The role {request.Role} is not valid");
            } 
        }
        
        user.UpdatedAt = DateTime.Now.ToUniversalTime();
        return user;
    }

    public static User ToUser(CreateUserRequest request)
    {
        User newUser = new User();
        newUser.Username = request.Username;
        newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        switch (request.Role.ToLower())
        {
            case "user":
                newUser.Role = Role.User;
                break;
            case "admin":
                newUser.Role = Role.Admin;
                break;
            case "superadmin":
                newUser.Role = Role.SuperAdmin;
                break;
            default:
                throw new InvalidUserException($"The role {request.Role} is not valid");
        }
        return newUser;
    }
}