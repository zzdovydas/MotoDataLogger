using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static List<User> _users = new List<User>();

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            if(_users.Count < 1){
                 _users.Add(new User{Username="test",PasswordHash= BCrypt.Net.BCrypt.HashPassword("password")});
            }
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto request)
        {
            if (_users.Any(u => u.Username == request.Username))
            {
                return BadRequest("User with this username already exists.");
            }

             if (request.Password != request.ConfirmPassword)
            {
                return BadRequest("Passwords does not match.");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _users.Add(user);
            return Ok($"User {user.Username} was registered successfully");
        }


        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto request)
        {
            var user = _users.FirstOrDefault(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                // You can add more claims here, such as roles.
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15), // Token expires in 15 minutes
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
