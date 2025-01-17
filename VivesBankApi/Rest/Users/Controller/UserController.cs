using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Service;

namespace VivesBankApi.Rest.Users.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(
            users.Select(
                user => UserMapper.ToUserResponse(user)
            )
        );
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(UserMapper.ToUserResponse(user));
    }

    [HttpPost]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdUser = await _userService.AddUserAsync(userRequest);
        return CreatedAtAction(
            nameof(GetUser), new { id = createdUser.Id }, 
            UserMapper.ToUserResponse(createdUser)
        );
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequest user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var updatedUser = await _userService.UpdateUserAsync(id, user);

        return Ok(UserMapper.ToUserResponse(updatedUser));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(String id, [FromQuery] bool logically = true)
    {
       await _userService.DeleteUserAsync(id, logically);
       return NoContent();
    }
}