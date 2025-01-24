using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotChocolate.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using StackExchange.Redis;
using VivesBankApi.Database;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Repository;
using VivesBankApi.Rest.Users.Validator;
using VivesBankApi.WebSocket.Model;
using VivesBankApi.WebSocket.Service;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Role = VivesBankApi.Rest.Users.Models.Role;

namespace VivesBankApi.Rest.Users.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDatabase _cache;
    private readonly AuthJwtConfig _authConfig;
    private readonly ILogger _logger;
    private readonly IWebsocketHandler _webSocketHandler;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(ILogger<UserService> logger, IUserRepository userRepository, AuthJwtConfig authConfig, IConnectionMultiplexer connectionMultiplexer, IWebsocketHandler webSocketHandler, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _authConfig = authConfig;
        _userRepository = userRepository;
        _cache = connectionMultiplexer.GetDatabase();
        _webSocketHandler = webSocketHandler;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<PagedList<UserResponse>> GetAllUsersAsync(
        int pageNumber, 
        int pageSize,
        string role,
        bool? isDeleted,
        string direction = "asc")
    {
        var users = await _userRepository.GetAllUsersPagedAsync(pageNumber, pageSize, role, isDeleted, direction);
        var mappedUsers = new PagedList<UserResponse>(
            users.Select(u => u.ToUserResponse()),
            users.TotalCount,
            users.PageNumber,
            users.PageSize
        );
        return mappedUsers;
    }
    
     public async Task<UserResponse> GetUserByIdAsync(string id)
    {
       var user = await GetByIdAsync(id) ?? throw new UserNotFoundException(id);
       return user.ToUserResponse();
    }

     [Authorize]
    public async Task<UserResponse> AddUserAsync(CreateUserRequest userRequest)
    {
        if (!UserValidator.ValidateDni(userRequest.Dni))
        {
            throw new  InvalidDniException(userRequest.Dni);
        }
        User newUser = userRequest.toUser();
        User? userWithTheSameUsername = await GetByUsernameAsync(userRequest.Dni);
        if (userWithTheSameUsername != null)
        {
            throw new UserAlreadyExistsException(userRequest.Dni);
        }
        await _userRepository.AddAsync(newUser);
        var notificacion = new Notification<UserResponse>
        {
            Type = Notification<UserResponse>.NotificationType.Create.ToString(),
            CreatedAt = DateTime.Now,
            Data = newUser.ToUserResponse()
        };
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        await _webSocketHandler.NotifyUserAsync(id,notificacion);
        return newUser.ToUserResponse();
    }
    

    public async Task<UserResponse> GetUserByUsernameAsync(string username)
    {
        var user = await GetByUsernameAsync(username) ?? throw new UserNotFoundException(username);
        return user.ToUserResponse();
    }

    public async Task<UserResponse> GettingMyUserData()
    {
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var res = await GetByIdAsync(id);
        return res.ToUserResponse();
    }


    public async Task<UserResponse> UpdateUserAsync(String id, UserUpdateRequest user)
    {
        if (user.Dni != null && !UserValidator.ValidateDni(user.Dni))
        {
             throw new InvalidDniException(user.Dni);
        }

        User? userToUpdate = await GetByIdAsync(id) ?? throw new UserNotFoundException(id);
        
        if (user.Dni != null)
        {
            User? userWithTheSameUsername = await GetByUsernameAsync(userToUpdate.Dni);
            if (userWithTheSameUsername != null && userWithTheSameUsername.Id != id)
            {
                throw new UserAlreadyExistsException(user.Dni);
            }
        }
        
        User updatedUser = user.UpdateUserFromInput(userToUpdate);
        await _userRepository.UpdateAsync(updatedUser);
        // Removing old cache entry
        await _cache.KeyDeleteAsync(id);
        await _cache.KeyDeleteAsync("users:" + userToUpdate.Dni.Trim().ToUpper());
        // Adding new cache entry
        await _cache.StringSetAsync(id, JsonConvert.SerializeObject(updatedUser), TimeSpan.FromMinutes(10));
        return updatedUser.ToUserResponse();
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
            await _cache.KeyDeleteAsync(id);
            await _cache.KeyDeleteAsync("users:" + userToUpdate.Dni.Trim().ToUpper());
        }
        else
        {
            await _cache.KeyDeleteAsync(id);
            await _cache.KeyDeleteAsync("users:" + userToUpdate.Dni.Trim().ToUpper());
            await _userRepository.DeleteAsync(id);
        }
    }

    private async Task<User?> GetByIdAsync(string id)
    {
        // Try to get from cache first
        var cachedUser = await _cache.StringGetAsync(id);
        if (!cachedUser.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<User>(cachedUser);
        }

        // If not in cache, get from DB and cache it
        User? user = await _userRepository.GetByIdAsync(id);
        if (user != null)
        {
            await _cache.StringSetAsync(id, JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(10));
            return user;
        }
        return null;
    }

    private async Task<User?> GetByUsernameAsync(string username)
    {
        // Try to get from cache first
        var cachedUser = await _cache.StringGetAsync("users:" + username.Trim().ToUpper());
        if (!cachedUser.IsNullOrEmpty)
        {
            return JsonConvert.DeserializeObject<User>(cachedUser);
        }
        // If not in cache, get from DB and cache it
        User? user = await _userRepository.GetByUsernameAsync(username);
        if (user != null)
        {
            await _cache.StringSetAsync("users:" + username.Trim().ToUpper(), JsonConvert.SerializeObject(user), TimeSpan.FromMinutes(10));
            return user;
        }
        return null;
    }

    public async Task<User?> LoginUser(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Dni);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return null;
        return user;
    }

    public async Task<User?> UpdateMyPassword(UpdatePasswordRequest request)
    {
        _logger.LogInformation("Updating my user profile");
        var user = _httpContextAccessor.HttpContext!.User;
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        User? userToUpdate = await GetByIdAsync(id) ?? throw new UserNotFoundException(id);
        userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await _userRepository.UpdateAsync(userToUpdate);
        await _cache.KeyDeleteAsync(id);
        await _cache.KeyDeleteAsync("users:" + userToUpdate.Dni.Trim().ToUpper());
        await _cache.StringSetAsync(id, JsonConvert.SerializeObject(userToUpdate), TimeSpan.FromMinutes(10));
        return userToUpdate;
    }

    public Task<User?> DeleteMeAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<User?> RegisterUser(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Dni);
        if (user!= null)
            throw new UserAlreadyExistsException(request.Dni);

        var newUser = request.ToUser();
        _logger.LogInformation("RegisterUser new id " + newUser.Id);
        newUser.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await _userRepository.AddAsync(newUser);
        return newUser;
    }
    
    

    public String GenerateJwtToken(User user)
    {
        _logger.LogInformation("Generating JWT token");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfig.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        _logger.LogInformation($"Inserting id to Claims: {user.Id}");
        var claims = new[]
        {
            new Claim("UserId", user.Id)
        };
        var token = new JwtSecurityToken(
            _authConfig.Issuer,
            _authConfig.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_authConfig.ExpiresInMinutes)),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}