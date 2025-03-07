//c:\Users\Dovydas\Documents\MotoDataLogger\API\MotoDataLoggerAPI.Tests\AuthControllerTests.cs
using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using MotoDataLoggerAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Text.Json;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class AuthControllerTests : IAsyncLifetime
    {
        private readonly AuthController _controller;
        private readonly IUserRepository _repository;
        private readonly MotoDataContext _context;

        public AuthControllerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Key", "super_secret_key_for_jwt_tokens_1234567890" },
                    { "Jwt:Issuer", "MotoDataLoggerAPI" },
                    { "Jwt:Audience", "MotoDataLoggerUsers" }
                })
                .Build();
            var options = new DbContextOptionsBuilder<MotoDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new MotoDataContext(options);
            _repository = new AuthRepository(_context);
            _controller = new AuthController(configuration, _repository);
        }
        private async Task ClearDatabase()
        {
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();
        }

        public async Task InitializeAsync()
        {
            await ClearDatabase();
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task Register_ValidUser_ReturnsOkResult()
        {
            var userRegisterDto = new UserRegisterDto
            {
                Username = "newuser",
                Password = "newpassword",
                ConfirmPassword = "newpassword"
            };

            var result = await _controller.Register(userRegisterDto);

            Assert.IsType<OkObjectResult>(result);

        }

        [Fact]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var existingUser = new UserRegisterDto
            {
                Username = "existinguser",
                Password = "password",
                ConfirmPassword = "password"
            };

            await _controller.Register(existingUser);

            var result = await _controller.Register(existingUser);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("User with this username already exists.", badRequestResult.Value);

        }

        [Fact]
        public async Task Register_PasswordsDoNotMatch_ReturnsBadRequest()
        {
            var userRegisterDto = new UserRegisterDto
            {
                Username = "newuser",
                Password = "password123",
                ConfirmPassword = "differentpassword"
            };

            var result = await _controller.Register(userRegisterDto);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Passwords do not match.", badRequestResult.Value);

        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkResultWithToken()
        {
            var existingUser = new UserRegisterDto
            {
                Username = "newuser2",
                Password = "password",
                ConfirmPassword = "password"
            };

            await _controller.Register(existingUser);
            var userLoginDto = new UserLoginDto
            {
                Username = "newuser2",
                Password = "password"
            };

            var result = await _controller.Login(userLoginDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var tokenResult = JsonSerializer.Deserialize(JsonSerializer.Serialize(okResult.Value), new { Token = "" }.GetType(), options);
            var tokenProperty = tokenResult.GetType().GetProperty("Token");
            Assert.NotNull(tokenProperty.GetValue(tokenResult));
        }

        [Fact]
        public async Task Login_InvalidUsername_ReturnsUnauthorized()
        {
            var userLoginDto = new UserLoginDto
            {
                Username = "invaliduser",
                Password = "password"
            };

            var result = await _controller.Login(userLoginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal("Invalid username or password.", unauthorizedResult.Value);

        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var existingUser = new UserRegisterDto
            {
                Username = "newuser3",
                Password = "password",
                ConfirmPassword = "password"
            };

            await _controller.Register(existingUser);
            var userLoginDto = new UserLoginDto
            {
                Username = "newuser3",
                Password = "wrongpassword"
            };

            var result = await _controller.Login(userLoginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal("Invalid username or password.", unauthorizedResult.Value);

        }
    }
}
