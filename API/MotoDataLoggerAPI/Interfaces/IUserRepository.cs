using MotoDataLoggerAPI.Models;

namespace MotoDataLoggerAPI.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<User> AddUserAsync(User user);
        Task<List<User>> GetUsersAsync();
    }
}
