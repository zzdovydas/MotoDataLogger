using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Reflection;
using System;
using System.Text.Json;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class AuthControllerTests : IDisposable
    {
        private readonly AuthController _controller;
        private static List<User> _users;

        public AuthControllerTests()
        {   
            // Create a mock configuration for testing
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?> 
                {
                    { "Jwt:Key", "super_secret_key_for_jwt_tokens_1234567890" },
                    { "Jwt:Issuer", "MotoDataLoggerAPI" },
                    { "Jwt:Audience", "MotoDataLoggerUsers" }
                })
                .Build();

            _controller = new AuthController(configuration);
            FieldInfo field = typeof(AuthController).GetField("_users", BindingFlags.NonPublic | BindingFlags.Static);
            _users = (List<User>)field.GetValue(null) ?? new List<User>(); 
            _users.Clear();
        }

        [Fact]
        public void Register_ValidUser_ReturnsOkResult()
        {
            var userRegisterDto = new UserRegisterDto
            {
                Username = "newuser",
                Password = "newpassword",
                ConfirmPassword = "newpassword"
            };

            var result = _controller.Register(userRegisterDto);

            Assert.IsType<OkObjectResult>(result);
           
        }

        [Fact]
        public void Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var existingUser = new UserRegisterDto
            {
                Username = "existinguser",
                Password = "password",
                ConfirmPassword = "password"
            };

            _controller.Register(existingUser);

            var result = _controller.Register(existingUser);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("User with this username already exists.", badRequestResult.Value);
            
        }

        [Fact]
        public void Register_PasswordsDoNotMatch_ReturnsBadRequest()
        {
            var userRegisterDto = new UserRegisterDto
            {
                Username = "newuser",
                Password = "password123",
                ConfirmPassword = "differentpassword"
            };

            var result = _controller.Register(userRegisterDto);

            Assert.IsType<BadRequestObjectResult>(result);
             var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Passwords does not match.", badRequestResult.Value);
           
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsOkResultWithToken()
        {
             var existingUser = new UserRegisterDto
            {
                Username = "newuser2",
                Password = "password",
                ConfirmPassword = "password"
            };

            _controller.Register(existingUser);
            var userLoginDto = new UserLoginDto
            {
                Username = "newuser2",
                Password = "password"
            };

            var result = _controller.Login(userLoginDto);

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
        public void Login_InvalidUsername_ReturnsUnauthorized()
        {
            var userLoginDto = new UserLoginDto
            {
                Username = "invaliduser",
                Password = "password"
            };

            var result = _controller.Login(userLoginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
           
        }

        [Fact]
        public void Login_InvalidPassword_ReturnsUnauthorized()
        {
            var existingUser = new UserRegisterDto
            {
                Username = "newuser3",
                Password = "password",
                ConfirmPassword = "password"
            };

            _controller.Register(existingUser);
            var userLoginDto = new UserLoginDto
            {
                Username = "newuser3",
                Password = "wrongpassword"
            };

            var result = _controller.Login(userLoginDto);

            Assert.IsType<UnauthorizedObjectResult>(result);
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal("Invalid username or password.", unauthorizedResult.Value);
           
        }
        public void Dispose()
        {
             _users.Clear();
        }
    }
}
