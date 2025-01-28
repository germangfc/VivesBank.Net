using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Middleware.Jwt;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Mapper;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;
using LoginRequest = VivesBankApi.Rest.Users.Dtos.LoginRequest;

namespace VivesBankApi.Rest.Users.Controller;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtGenerator _jwtGenerator;
    
    public UserController(IUserService userService, IJwtGenerator jwtGenerator)
    {
        _userService = userService;
        _jwtGenerator = jwtGenerator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _userService.RegisterUser(request);
            var token = _jwtGenerator.GenerateJwtToken(user);
            return Ok(new { token });
        }
        catch (UserAlreadyExistsException e)
        {
            return Unauthorized(e.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
 
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.LoginUser(request);
        if (user == null) return Unauthorized("Invalid username or password");
        var token = _jwtGenerator.GenerateJwtToken(user);
        return Ok(new { token });
    }
    
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<UserResponse>>> GetAllUsersAsync(
        [FromQuery ]int pageNumber = 0, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string role = "",
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string direction = "asc")
    {
        PagedList<UserResponse> pagedList =  await _userService.GetAllUsersAsync(
            pageNumber, pageSize, role, isDeleted, direction
        );

        return new PageResponse<UserResponse>
        {
            Content = pagedList.ToList(),
            TotalPages = pagedList.PageCount,
            TotalElements = pagedList.TotalCount,
            PageSize = pagedList.PageSize,
            PageNumber = pagedList.PageNumber,
            TotalPageElements = pagedList.Count,
            Empty = pagedList.Count == 0,
            First = pagedList.IsFirstPage,
            Last = pagedList.IsLastPage,
            SortBy = "username",
            Direction = direction
        };
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }
    
    [HttpGet("me")]
    [Authorize(Policy = "AdminOrUserPolicy")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userService.GettingMyUserData();
        return Ok(result);
    }
    
    
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return Ok(user);
    }
    
    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> AddUser([FromBody] CreateUserRequest userRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdUser = await _userService.AddUserAsync(userRequest);
        return CreatedAtAction(
            nameof(GetUser), new { id = createdUser.Id }, 
            createdUser
        );
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateRequest user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var updatedUser = await _userService.UpdateUserAsync(id, user);

        return Ok(updatedUser);
    }

    [HttpPut("password")]
    [Authorize(Policy = "AdminOrUserPolicy")]
    public async Task<IActionResult> ChangePassword(UpdatePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        await _userService.UpdateMyPassword(request);
        return NoContent();
    }
    
    [HttpDelete("baja")]
    [Authorize(Policy = "AdminOrUserPolicy")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        await _userService.DeleteMeAsync();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(String id, [FromQuery] bool logically = true)
    {
        await _userService.DeleteUserAsync(id, logically);
        return NoContent();
    }
}