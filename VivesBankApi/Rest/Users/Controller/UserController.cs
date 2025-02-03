using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Middleware.Jwt;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Service;
using LoginRequest = VivesBankApi.Rest.Users.Dtos.LoginRequest;

namespace VivesBankApi.Rest.Users.Controller;

/// <summary>
/// Controlador para gestionar usuarios en la aplicación.
/// Proporciona métodos para registrar, iniciar sesión, obtener información de usuarios, y administrar perfiles de usuario.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtGenerator _jwtGenerator;
    
    /// <summary>
    /// Constructor del controlador `UserController`.
    /// </summary>
    /// <param name="userService">Servicio para gestionar operaciones de usuario.</param>
    /// <param name="jwtGenerator">Generador de tokens JWT.</param>
    public UserController(IUserService userService, IJwtGenerator jwtGenerator)
    {
        _userService = userService;
        _jwtGenerator = jwtGenerator;
    }

    /// <summary>
    /// Registra un nuevo usuario y devuelve un token JWT.
    /// </summary>
    /// <param name="request">Datos del usuario para registrar.</param>
    /// <returns>Token JWT del usuario registrado.</returns>
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

    /// <summary>
    /// Inicia sesión de un usuario y devuelve un token JWT.
    /// </summary>
    /// <param name="request">Datos de inicio de sesión.</param>
    /// <returns>Token JWT del usuario autenticado.</returns>
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

    /// <summary>
    /// Obtiene todos los usuarios registrados (solo accesible para administradores).
    /// Soporta paginación y filtrado por rol, estado de eliminación y dirección de ordenamiento.
    /// </summary>
    /// <param name="pageNumber">Número de página.</param>
    /// <param name="pageSize">Tamaño de página.</param>
    /// <param name="role">Filtro por rol.</param>
    /// <param name="isDeleted">Filtro por estado de eliminación.</param>
    /// <param name="direction">Dirección de ordenamiento ("asc" o "desc").</param>
    /// <returns>Lista paginada de usuarios.</returns>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<PageResponse<UserResponse>>> GetAllUsersAsync(
        [FromQuery] int pageNumber = 0, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string role = "",
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string direction = "asc")
    {
        PagedList<UserResponse> pagedList = await _userService.GetAllUsersAsync(
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
            SortBy = "dni",
            Direction = direction
        };
    }

    /// <summary>
    /// Obtiene la información de un usuario por su ID (solo accesible para administradores).
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <returns>Datos del usuario solicitado.</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> GetUser(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return Ok(user);
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado (solo accesible para el usuario y administradores).
    /// </summary>
    /// <returns>Datos del perfil del usuario autenticado.</returns>
    [HttpGet("me")]
    [Authorize(Policy = "AdminOrUserPolicy")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _userService.GettingMyUserData();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario (solo accesible para administradores).
    /// </summary>
    /// <param name="username">Nombre de usuario del usuario.</param>
    /// <returns>Datos del usuario.</returns>
    [HttpGet("username/{username}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult> GetUserByUsername(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return Ok(user);
    }

    /// <summary>
    /// Agrega un nuevo usuario (solo accesible para administradores).
    /// </summary>
    /// <param name="userRequest">Datos del nuevo usuario.</param>
    /// <returns>Usuario creado.</returns>
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

    /// <summary>
    /// Actualiza los datos de un usuario por su ID (solo accesible para administradores).
    /// </summary>
    /// <param name="id">ID del usuario a actualizar.</param>
    /// <param name="user">Datos del usuario a actualizar.</param>
    /// <returns>Usuario actualizado.</returns>
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

    /// <summary>
    /// Cambia la contraseña del usuario autenticado (solo accesible para el usuario y administradores).
    /// </summary>
    /// <param name="request">Nueva contraseña.</param>
    /// <returns>Respuesta vacía (NoContent).</returns>
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

    /// <summary>
    /// Elimina la cuenta del usuario autenticado (solo accesible para el usuario y administradores).
    /// </summary>
    /// <returns>Respuesta vacía (NoContent).</returns>
    [HttpDelete("baja")]
    [Authorize(Policy = "AdminOrUserPolicy")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        await _userService.DeleteMeAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina un usuario por su ID (solo accesible para administradores).
    /// </summary>
    /// <param name="id">ID del usuario a eliminar.</param>
    /// <param name="logically">Indica si la eliminación es lógica (por defecto es verdadera).</param>
    /// <returns>Respuesta vacía (NoContent).</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteUser(string id, [FromQuery] bool logically = true)
    {
        await _userService.DeleteUserAsync(id, logically);
        return NoContent();
    }
}
