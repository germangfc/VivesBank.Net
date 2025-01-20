using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Users.Dtos;
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
            SortBy = "id",
            Direction = direction
        };
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }
    
    [HttpGet("username/{username}")]
    public async Task<ActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return Ok(user);
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
            createdUser
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

        return Ok(updatedUser);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(String id, [FromQuery] bool logically = true)
    {
        await _userService.DeleteUserAsync(id, logically);
        return NoContent();
    }
}