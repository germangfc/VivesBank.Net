using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework.Legacy;
using VivesBankApi.Rest.Users.Controller;
using VivesBankApi.Rest.Users.Dtos;
using VivesBankApi.Rest.Users.Exceptions;
using VivesBankApi.Rest.Users.Models;
using VivesBankApi.Rest.Users.Service;

namespace Tests.Rest.Users.Controller
{
    public class UserControllerTest
    {
        private Mock<IUserService> _service;
        private UserController _userController;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IUserService>();
            _userController = new UserController(_service.Object);
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsOk_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserResponse>
            {
                new UserResponse { Id = "1", Dni = "TestUser1", Role = Role.User.ToString() },
                new UserResponse { Id = "2", Dni = "TestUser2", Role = Role.Admin.ToString() }
            };

            var pagedList = new PagedList<UserResponse>(users, 2, 10, 2);

            _service.Setup(s => s.GetAllUsersAsync(0, 10, "", null, "asc")).ReturnsAsync(pagedList);

            // Act
            var result = await _userController.GetAllUsersAsync(0, 10, "", null, "asc");

            // Assert
            ClassicAssert.NotNull(result);
            var returnedPageResponse = result.Value;
            ClassicAssert.NotNull(returnedPageResponse);
            ClassicAssert.AreEqual(2, returnedPageResponse.TotalElements);
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsEmptyPage_WhenNoUsersExist()
        {
            // Arrange
            var pagedList = new PagedList<UserResponse>(new List<UserResponse>(), 0, 10, 0);

            _service.Setup(s => s.GetAllUsersAsync(0, 10, "", null, "asc")).ReturnsAsync(pagedList);

            // Act
            var result = await _userController.GetAllUsersAsync(0, 10, "", null, "asc");

            // Assert
            ClassicAssert.NotNull(result);
            var returnedPageResponse = result.Value as PageResponse<UserResponse>;
            ClassicAssert.NotNull(returnedPageResponse);
            ClassicAssert.AreEqual(0, returnedPageResponse.TotalElements);
        }
        
        [Test]
        public async Task GetUser_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var user = new UserResponse { Id = "1", Dni = "TestUser", Role = Role.User.ToString() };

            _service.Setup(s => s.GetUserByIdAsync("1")).ReturnsAsync(user);

            // Act
            var result = await _userController.GetUser("1") as OkObjectResult;

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(200, result.StatusCode);
            var returnedUser = result.Value as UserResponse;
            ClassicAssert.AreEqual(user.Dni, returnedUser.Dni);
        }

        [Test]
        public async Task GetUserByUsername_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var user = new UserResponse
            {
                Id = "1",
                Dni = "testuser",
                Role = Role.User.ToString()
            };

            _service.Setup(s => s.GetUserByUsernameAsync(username)).ReturnsAsync(user);

            // Act
            var result = await _userController.GetUserByUsername(username) as OkObjectResult;

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(200, result.StatusCode);
            var returnedUser = result.Value as UserResponse;
            ClassicAssert.AreEqual(user.Dni, returnedUser.Dni);
        }

        [Test]
        public async Task GetUserByUsername_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "nonexistentuser";

            _service.Setup(s => s.GetUserByUsernameAsync(username)).ThrowsAsync(new UserNotFoundException("User not found"));

            // Act
            IActionResult result = null;
            try
            {
                result = await _userController.GetUserByUsername(username);
            }
            catch (UserNotFoundException e)
            {
                result = new NotFoundObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        }
        
        [Test]
        public async Task AddUser_ReturnsCreated()
        {
            // Arrange
            var userRequest = new CreateUserRequest
            {
                Dni = "TestUser",
                Password = "aPassword",
                Role = "user"
            };

            var createdUser = new UserResponse
            {
                Id = "1",
                Dni = "TestUser",
                Role = Role.User.ToString()
            };

            _service.Setup(s => s.AddUserAsync(userRequest)).ReturnsAsync(createdUser);

            // Act
            var result = await _userController.AddUser(userRequest) as CreatedAtActionResult;

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(201, result.StatusCode);
            var returnedUser = result.Value as UserResponse;
            ClassicAssert.AreEqual(createdUser.Dni, returnedUser.Dni);
        }
        
        [Test]
        public async Task AddUser_ReturnsConflict_WhenUserAlreadyExists()
        {
            // Arrange
            var userRequest = new CreateUserRequest
            {
                Dni = "TestUser",
                Password = "aPassword",
                Role = "user"
            };

            _service.Setup(s => s.AddUserAsync(userRequest)).ThrowsAsync(new UserAlreadyExistsException("User already exists"));

            // Act
            IActionResult result = null;
            try
            {
                await _userController.AddUser(userRequest);
            }
            catch (Exception e)
            {
                result = new ConflictObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<ConflictObjectResult>(result);
        }

        [Test]
        public async Task AddUser_ReturnsBadRequest_WhenDniIsInvalid()
        {
            // Arrange
            var userRequest = new CreateUserRequest
            {
                Dni = "InvalidDni",
                Password = "aPassword",
                Role = "user"
            };
            
            _service.Setup(s => s.AddUserAsync(userRequest)).ThrowsAsync(new InvalidUsernameException("Username is invalid"));

            // Act
            IActionResult result = null;
            try
            {
                await _userController.AddUser(userRequest);
            }
            catch (Exception e)
            {
                result = new BadRequestObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task AddUser_ReturnsBadRequest_WhenUserIsInvalid()
        {
            // Arrange
            CreateUserRequest request = new CreateUserRequest
            {
                Password = "aPassword",
                Role = "user"
            };

            _userController.ModelState.AddModelError("Username", "Required");

            // Act
            var result = await _userController.AddUser(request);

            // Assert
            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }
        
        [Test]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userUpdateRequest = new UserUpdateRequest
            {
                Username = "UpdatedUser",
                Role = "admin"
            };
            _service.Setup(s => s.UpdateUserAsync("1", userUpdateRequest))
                .ThrowsAsync(new UserNotFoundException("User not found"));

            // Act
            IActionResult result = null;
            try
            {
                result = await _userController.UpdateUser("1", userUpdateRequest);
            }
            catch (UserNotFoundException e)
            {
                result = new NotFoundObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task UpdateUser_ReturnsBadRequest_WhenUsernameIsNotaDNI()
        {
            // Arrange
            var userUpdateRequest = new UserUpdateRequest
            {
                Username = "1234567890",
                Password = "password",
                Role = "admin"
            };
            _service.Setup(s => s.UpdateUserAsync("1", userUpdateRequest))
                .ThrowsAsync(new InvalidUsernameException("Invalid username"));

            // Act
            IActionResult result = null;
            try
            {
                result = await _userController.UpdateUser("1", userUpdateRequest);
            }
            catch (InvalidUsernameException e)
            {
                result = new BadRequestObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
        }
        [Test]
        public async Task UpdateUser_ReturnsOkResult()
        {
            // Arrange
            var userUpdateRequest = new UserUpdateRequest
            {
                Username = "UpdatedUser",
                Role = "admin"
            };

            var updatedUser = new UserResponse
            {
                Id = "1",
                Dni = "UpdatedUser",
                Role = Role.Admin.ToString()
            };

            _service.Setup(s => s.UpdateUserAsync("1", userUpdateRequest)).ReturnsAsync(updatedUser);

            // Act
            var result = await _userController.UpdateUser("1", userUpdateRequest) as OkObjectResult;

            // Assert
            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(200, result.StatusCode);
            var returnedUser = result.Value as UserResponse;
            ClassicAssert.AreEqual(updatedUser.Dni, returnedUser.Dni);
        }

        [Test]
        public async Task DeleteUser_ReturnsNoContent()
        {
            // Arrange
            _service.Setup(s => s.DeleteUserAsync("1", true)).Returns(Task.CompletedTask);

            // Act
            var result = await _userController.DeleteUser("1");

            // Assert
            ClassicAssert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _service.Setup(s => s.DeleteUserAsync("1", true)).ThrowsAsync(new UserNotFoundException("User not found"));

            // Act 
            IActionResult result = null;
            try
            {
                result = await _userController.DeleteUser("1");
            }
            catch (UserNotFoundException e)
            {
                result = new NotFoundObjectResult(new { error = e.Message });
            }

            // Assert
            ClassicAssert.IsInstanceOf<NotFoundObjectResult>(result);
        }
    }
}
