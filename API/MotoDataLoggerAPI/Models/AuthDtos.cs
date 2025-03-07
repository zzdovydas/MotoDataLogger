//c:\Users\Dovydas\Documents\MotoDataLogger\API\MotoDataLoggerAPI\Models\AuthDtos.cs
using System.ComponentModel.DataAnnotations;

namespace MotoDataLoggerAPI.Models
{
    public class UserLoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
     public class UserRegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class User
    {
        [Key]
        public int Id {get; set;}
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
    }
}
