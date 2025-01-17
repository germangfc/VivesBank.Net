using System.Text.Json;
using StackExchange.Redis;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Validator;

namespace VivesBankApi.Rest.Users.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDatabase _cache;
    
    public UserService(IUserRepository userRepository, IConnectionMultiplexer connectionMultiplexer)
    {
        _userRepository = userRepository;
        _cache = connectionMultiplexer.GetDatabase();
    }
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        //Attempting to find it in the cache first
        var cachedUser = await _cache.StringGetAsync(id);
        if (!cachedUser.IsNullOrEmpty)
        {
            Console.WriteLine("Found cached user");
            var json = await _cache.StringGetAsync(id);
            
            if (!json.IsNullOrEmpty)
            {
                return JsonSerializer.Deserialize<User>(json);
            }
        }

        // If not in cache, get from DB and cache it
        User? user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _cache.StringSetAsync(id, JsonSerializer.Serialize(user), TimeSpan.FromMinutes(10));
        }
        return user;
    }

    public async Task<User> AddUserAsync(CreateUserRequest userRequest)
    {
        if (!UserValidator.ValidateDni(userRequest.Username))
        {
            throw new  InvalidUserException($"The DNI {userRequest.Username} is not valid");
        }
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
        if (user.Username != null && !UserValidator.ValidateDni(user.Username))
        {
             throw new InvalidUserException($"The DNI {user.Username} is not valid");
        }
        
        User? userToUpdate = await GetUserByIdAsync(id);
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
        await _cache.KeyDeleteAsync(id);
        await _cache.StringSetAsync(id, JsonSerializer.Serialize(updatedUser), TimeSpan.FromMinutes(10));
        return updatedUser;
    }

    public async Task DeleteUserAsync(String id, bool logically)
    {
        User? userToUpdate = await GetUserByIdAsync(id);
        if (userToUpdate == null)
        {
            throw new UserNotFoundException(id);
        }

        if (logically)
        {
            userToUpdate.IsDeleted = true;
            await _userRepository.UpdateAsync(userToUpdate);
            await _cache.KeyDeleteAsync(id);
        }
        else
        {
            await _cache.KeyDeleteAsync(id);
            await _userRepository.DeleteAsync(id);
        }
    }
}