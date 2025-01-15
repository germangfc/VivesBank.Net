using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;

namespace VivesBankApi.Rest.Users.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User> AddUserAsync(CreateUserRequest userRequest)
    {
        User newUser = UserMapper.ToUser(userRequest);
        User? userWithTheSameUsername = await _userRepository.GetByUsernameAsync(userRequest.Username);
        if (userWithTheSameUsername != null)
        {
            throw new UserAlreadyExistsException(userRequest.Username);
        }
        await _userRepository.AddAsync(newUser);
        return newUser;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }


    public async Task<User> UpdateUserAsync(String id, UserUpdateRequest user)
    {
        User? userToUpdate = await _userRepository.GetByIdAsync(id);
        
        if (userToUpdate == null)
        {
            throw new UserNotFoundException(id);
        }

        if (user.Username != null)
        {
            User? userWithTheSameUsername = await _userRepository.GetByUsernameAsync(user.Username);
            if (userWithTheSameUsername!= null && userWithTheSameUsername.Id != id)
            {
                throw new UserAlreadyExistsException(user.Username);
            }
        }
        
        User updatedUser = UserMapper.UpdateUserFromInput(user, userToUpdate);
        await _userRepository.UpdateAsync(updatedUser);
        return updatedUser;
    }

    public async Task DeleteUserAsync(String id, bool logically)
    {
        User? userToUpdate = await _userRepository.GetByIdAsync(id);
        if (userToUpdate == null)
        {
            throw new UserNotFoundException(id);
        }

        if (logically)
        {
            userToUpdate.IsDeleted = true;
            await _userRepository.UpdateAsync(userToUpdate);
        }
        else
        {
            await _userRepository.DeleteAsync(id);
        }
    }
}