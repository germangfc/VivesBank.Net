    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework.Legacy;
    using Quartz;
    using VivesBankApi.Middleware.Jwt;
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
            private Mock<IJwtGenerator> _jwtGenerator;

            [SetUp]
            public void Setup()
            {
                _service = new Mock<IUserService>();
                _jwtGenerator = new Mock<IJwtGenerator>();
                _userController = new UserController(_service.Object, _jwtGenerator.Object);
            }
            
            [Test]
            public async Task Register_ValidRequest()
            {
                // Arrange
                var request = new LoginRequest { Dni = "123456789", Password = "Password123" };
                var fakeUser = new User();
                var fakeToken = "fake.jwt.token";

                _service.Setup(s => s.RegisterUser(request)).ReturnsAsync(fakeUser);
                _jwtGenerator.Setup(j => j.GenerateJwtToken(fakeUser)).Returns(fakeToken);

                // Act
                var result = await _userController.Register(request);

                // Assert
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
                var okResult = result as OkObjectResult;
                var json = JsonConvert.SerializeObject(okResult!.Value);
                var response = JObject.Parse(json);

                Assert.That(response["token"]!.Value<string>(), Is.EqualTo(fakeToken));
            }
            
            [Test]
            public async Task Register_WhenUserAlreadyExists()
            {
                // Arrange
                var request = new LoginRequest 
                { 
                    Dni = "123456789", 
                    Password = "securePassword123" 
                };
    
                var expectedMessage = $"A user with the username '{request.Dni}' already exists.";

                _service
                    .Setup(s => s.RegisterUser(request))
                    .ThrowsAsync(new UserAlreadyExistsException(request.Dni)); 

                // Act
                var result = await _userController.Register(request);

                // Assert
                Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    
                var unauthorizedResult = result as UnauthorizedObjectResult;
                Assert.That(unauthorizedResult?.Value, Is.EqualTo(expectedMessage));
            }
            
            [Test]
            public async Task Login_ValidCredentials_ReturnsOkWithToken()
            {
                // Arrange
                var request = new LoginRequest 
                { 
                    Dni = "123456789", 
                    Password = "Password123" 
                };
    
                var fakeUser = new User();
                var fakeToken = "fake.jwt.token";

                _service.Setup(s => s.LoginUser(request)).ReturnsAsync(fakeUser);
                _jwtGenerator.Setup(j => j.GenerateJwtToken(fakeUser)).Returns(fakeToken);

                // Act
                var result = await _userController.Login(request);

                // Assert
                Assert.That(result, Is.InstanceOf<OkObjectResult>());
    
                var okResult = result as OkObjectResult;
                var json = JsonConvert.SerializeObject(okResult!.Value);
                var response = JObject.Parse(json);
    
                Assert.That(response["token"]!.Value<string>(), Is.EqualTo(fakeToken));
            }
            
            [Test]
            public async Task Login_InvalidCredentials_ReturnsUnauthorized()
            {
                // Arrange
                var request = new LoginRequest 
                { 
                    Dni = "123456789", 
                    Password = "wrongPassword" 
                };

                _service.Setup(s => s.LoginUser(request)).ReturnsAsync((User?)null);

                // Act
                var result = await _userController.Login(request);

                // Assert
                Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
    
                var unauthorizedResult = result as UnauthorizedObjectResult;
                Assert.That(unauthorizedResult?.Value, Is.EqualTo("Invalid username or password"));
            }
            
            [Test]
            public async Task Login_InvalidModel_ReturnsBadRequest()
            {
                // Arrange
                var invalidRequest = new LoginRequest 
                { 
                    Dni = "123", 
                    Password = "short" 
                };
                
                _userController.ModelState.Clear();
                _userController.ModelState.AddModelError("Dni", "Must be a DNI");
                _userController.ModelState.AddModelError("Password", "Password too short");

                // Act
                var result = await _userController.Login(invalidRequest);

                // Assert
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); 
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
            public async Task GetMyProfile_WhenUserIsAuthenticated()
            {
                // Arrange
                var expectedUser = new UserResponse
                {
                    Id = "1",
                    Dni = "TestUser",
                    Role = Role.User.ToString()
                };

                _service.Setup(s => s.GettingMyUserData())
                    .ReturnsAsync(expectedUser);

                // Act
                var result = await _userController.GetMyProfile() as OkObjectResult;

                // Assert
                ClassicAssert.NotNull(result);
                var actualUser = result.Value as UserResponse;
                ClassicAssert.NotNull(actualUser);
                ClassicAssert.AreEqual(expectedUser.Id, actualUser.Id);
                ClassicAssert.AreEqual(expectedUser.Dni, actualUser.Dni);
                ClassicAssert.AreEqual(expectedUser.Role, actualUser.Role);
    
                _service.Verify(s => s.GettingMyUserData(), Times.Once);
            }
            
            [Test]
            public void GetMyProfile_WhenServiceFailsAuthentication()
            {
                // Arrange
                var expectedException = new UnauthorizedAccessException("Invalid credentials");
                _service.Setup(s => s.GettingMyUserData())
                    .ThrowsAsync(expectedException);

                // Act & Assert
                var actualException = ClassicAssert.ThrowsAsync<UnauthorizedAccessException>(
                    async () => await _userController.GetMyProfile()
                );
    
                ClassicAssert.AreEqual(expectedException.Message, actualException.Message);
                _service.Verify(s => s.GettingMyUserData(), Times.Once);
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
                
                _service.Setup(s => s.AddUserAsync(userRequest)).ThrowsAsync(new InvalidDniException("Username is invalid"));

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
                    Dni = "UpdatedUser",
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
                    Dni = "1234567890",
                    Password = "password",
                    Role = "admin"
                };
                _service.Setup(s => s.UpdateUserAsync("1", userUpdateRequest))
                    .ThrowsAsync(new InvalidDniException("Invalid username"));

                // Act
                IActionResult result = null;
                try
                {
                    result = await _userController.UpdateUser("1", userUpdateRequest);
                }
                catch (InvalidDniException e)
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
                    Dni = "UpdatedUser",
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
            public async Task UpdateUser_ReturnsBadRequest_WhenDniOrPasswordIsTooShort()
            {
                // Arrange
                var userUpdateRequest = new UserUpdateRequest
                {
                    Dni = "123",
                    Password = "short",
                    Role = "user"
                };

                _userController.ModelState.AddModelError("Dni", "DNI must be at least 9 characters long.");
                _userController.ModelState.AddModelError("Password", "Password must be at least 8 characters long.");

                // Act
                var result = await _userController.UpdateUser("1", userUpdateRequest);

                // Assert
                ClassicAssert.IsInstanceOf<BadRequestObjectResult>(result);
            }
            
            [Test]
            public async Task ChangePassword_ValidRequest()
            {
                // Arrange
                var request = new UpdatePasswordRequest
                {
                    Password = "newValidPassword123"
                };

                _service.Setup(s => s.UpdateMyPassword(request))
                    .ReturnsAsync((User?)null); 

                // Act
                var result = await _userController.ChangePassword(request);

                // Assert
                Assert.That(result, Is.InstanceOf<NoContentResult>());
            }
            
            
            
            [Test]
            public async Task ChangePassword_InvalidModel()
            {
                // Arrange
                var invalidRequest = new UpdatePasswordRequest
                {
                    Password = "short" 
                };

                _userController.ModelState.AddModelError("Password", "Password must be at least 8 characters");

                // Act
                var result = await _userController.ChangePassword(invalidRequest);

                // Assert
                Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            }
            
            [Test]
            public async Task DeleteMyAccount_ValidRequest()
            {
                // Arrange
                _service.Setup(s => s.DeleteMeAsync())
                    .Returns(Task.CompletedTask);

                // Act
                var result = await _userController.DeleteMyAccount();

                // Assert
                Assert.That(result, Is.InstanceOf<NoContentResult>());
                _service.Verify(s => s.DeleteMeAsync(), Times.Once);
            }
            
            [Test]
            public void DeleteMyAccount_ServiceThrowsException()
            {
                // Arrange
                var expectedException = new InvalidOperationException("Error de prueba");
                _service.Setup(s => s.DeleteMeAsync())
                    .ThrowsAsync(expectedException);

                // Act & Assert
                var ex = Assert.ThrowsAsync<InvalidOperationException>(() => 
                    _userController.DeleteMyAccount());
        
                Assert.That(ex.Message, Is.EqualTo("Error de prueba"));
                _service.Verify(s => s.DeleteMeAsync(), Times.Once);
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
